using UnityEngine;
using UnityEngine.Tilemaps;

/* CREDITS:
* Most code by Pav, https://pavcreations.com/procedural-generation-of-2d-maps-in-unity/3/
* with indicated functions adapted from Unity Learn, https://learn.unity.com/course/2d-roguelike-tutorial/tutorial/add-a-game-board?version=6.0
* Comments by me to improve and prove my understanding.
* In retrospect, I should have left these as two separate scripts.
*/

public class BoardManager : MonoBehaviour
{
    //each cell is either passable or not; Player class reads this
    public class CellData {
        public bool Passable;
        public GameObject ContainedObject;
    }

    public PlayerControl player;
    [Header("Prefabs")]
    public GameObject foodPrefab;
    public int foodCount;
    public GameObject[] weaponPrefabs;
    public int weaponCount;

    //2D arrays
    int[,] generatedMap;    // 1s and 0s used for generating map; when complete, passes to below
    public CellData[,] boardData { get; private set; }    // 1/0s translated to Passable true/false, for ease of player
    private Tilemap tileMap;    // draws 1/0s as tiles
    private Grid grid;      // parent of tileMap that the Player can see as a phyical location

    //CA parameters
    [Header("Tiles")]
    public Tile[] groundTiles;
    public Tile[] wallTiles;
    [Header("Board Properties")]
    public int width;
    public int height;
    public string seed;
    private System.Random pseudoRandom;
    [Range(0,100)] public int fillingPercentage;

    //Hilbert's curve parameters
    Vector2[] hilbertPoints;
    int[,] hilbertPointsInt;
    int[,] hilbertNegativePointsInt;
    public int hilbertReps;
    public int hilbertScale;
    int hilbertIndx = 0;
    [SerializeField] int shiftX = 0, shiftY = 0;
    public int pathGirth;
    public int negativePathGirth;

    public bool isDisplayGuidelines;


    
    void Start() {
        tileMap = GetComponentInChildren<Tilemap>();
        grid = GetComponentInChildren<Grid>();

        CellularAutomata();
        DrawMap();
        GenerateFood();
        GenerateWeapons();

        if (player != null) {
            player.Spawn(FindFirstPassable());
        } else {
            Debug.Log("No player in board!");
        }
    }


    // Generate the cells
    void CellularAutomata() {
        //if no seed, create a seed from the clock
        seed = (seed.Length <= 0) ? Time.time.ToString() : seed;
        //generate a PRNG
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateGuidelines();
        GenerateMap();

        //clean up map
        for (int i = 0; i < 5; i++) { SmoothMap(); }
        RemoveSecludedCells();
        RecoverEdgeCells();
    }


    // Ignoring the edges, randomize which cells are filled, with a control on probability
    void GenerateMap() {
        generatedMap = new int[width,height];

        for (int x = 1; x < width-1; x++) {
            for (int y = 1; y < height-1; y++) {
                generatedMap[x,y] = (pseudoRandom.Next(0,100) < fillingPercentage) ? 1 : 0;
            }
        }
    }


    // Ignoring the edges, if a cell has >4 filled neighbors, fill it; <4, empty it; exactly 4, keep as-is
    void SmoothMap() {
        for (int x = 1; x < width-1; x++) {
            for (int y = 1; y < height-1; y++) {
                int neighbors = GetNeighborsCellCount(x,y,generatedMap);

                if (neighbors > 4) { generatedMap[x,y] = 1; }
                else if (neighbors < 4) { generatedMap[x,y] = 0; }

                //clear out the negative of Hilbert's Path
                generatedMap[x, y] = (hilbertNegativePointsInt[x, y] == 1) ? 0 : generatedMap[x, y];

                //fill in Hilbert's Path
                if (hilbertPointsInt[x,y] == 1) {
                    generatedMap[x,y] = 1;
                    if (neighbors > 4) {
                        FloodFillNeighboursWithValue(x,y,1);
                    }
                }
            }
        }
    }


    // Count how many filled neighbors a cell has
    int GetNeighborsCellCount(int x, int y, int[,] map) {
        int neighbors = 0;
        for (int i = -1; i <=1; i++) {
            for (int j = -1; j <=1; j++) {
                neighbors += map[i+x, j+y];     //+1 if filled, +0 if not
            }
        }
        neighbors -= map[x,y];      //don't count the cell itself
        return neighbors;
    }


    // Empty all edge cells
    void RecoverEdgeCells() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x == 0 || x == width-1 || y == 0 || y == height-1) {
                    generatedMap[x,y] = 0;
                }
            }
        }
    }


    // Ignoring the edges, empty all cells with no filled neighbors
    void RemoveSecludedCells() {
        for (int x = 1; x < width-1; x++) {
            for (int y = 1; y < height-1; y++) {
                generatedMap[x, y] = (GetNeighborsCellCount(x, y, generatedMap) <= 0) ? 0 : generatedMap[x, y];
            }
        }
    }


    // This pattern makes the caves more "natural" and connected
    void HilbertCurve(float x, float y, float xi, float xj, float yi, float yj, int n) {
        //Original algorithm by Andrew Cumming, http://www.fundza.com/algorithmic/space_filling/hilbert/basics/
        //via Pav
        if (n <= 0) {
            float X = x + (xi + yi) / 2;
            float Y = y + (xj + yj) / 2;
            hilbertPoints[hilbertIndx] = new Vector2((int)X * hilbertScale + shiftX,
                                                     (int)Y * hilbertScale + shiftY);
            hilbertIndx++;
        } else {
            HilbertCurve(x, y, yi / 2, yj / 2, xi / 2, xj / 2, n - 1);
            HilbertCurve(x + xi / 2, y + xj / 2, xi / 2, xj / 2, yi / 2, yj / 2, n - 1);
            HilbertCurve(x + xi / 2 + yi / 2, y + xj / 2 + yj / 2, xi / 2, xj / 2, yi / 2, yj / 2, n - 1);
            HilbertCurve(x + xi / 2 + yi, y + xj / 2 + yj, -yi / 2, -yj / 2, -xi / 2, -xj / 2, n - 1);
        }
    }


    void GenerateGuidelines()
    {
        // 4 points per one curve segment chunk
        hilbertPoints    = new Vector2[(int)Mathf.Pow(4, hilbertReps)]; 
        hilbertPointsInt = new int[Mathf.Max(width, height), Mathf.Max(width, height)];
        hilbertNegativePointsInt = new int[width, height];
        hilbertIndx      = 0;
        shiftX           = pseudoRandom.Next(-Mathf.Max(width, height) * (hilbertScale - 1), 0);
        shiftY           = pseudoRandom.Next(-Mathf.Max(width, height) * (hilbertScale - 1), 0);
    
        HilbertCurve(0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                    0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                    hilbertReps);
    
        // clear curve's grid by setting all cells to 0
        for (int x = 1; x < width-1; x++)
        {
            for (int y = 1; y < height-1; y++)
            {
                hilbertPointsInt[x, y] = 0;
            }
        }
    
        // hilbert curve connection nodes
        for(int i = 0; i < hilbertPoints.Length; i++)
        {
            int x = (int)hilbertPoints[i].x;
            int y = (int)hilbertPoints[i].y;
    
            if (x < Mathf.Max(width, height) && x >= 0 &&
                y < Mathf.Max(width, height) && y >= 0)
            {
                hilbertPointsInt[x, y] = 1;
            }  
        }
    
        // filling in all cells between hilbert curve nodes
        for (int i = 0; i < hilbertPoints.Length - 1; i++)
        {
            int x_curr = (int)hilbertPoints[i].x;
            int y_curr = (int)hilbertPoints[i].y;
            int x_next = (int)hilbertPoints[i + 1].x;
            int y_next = (int)hilbertPoints[i + 1].y;
            int x_dist = (int)Mathf.Abs(x_curr - x_next);
            int y_dist = (int)Mathf.Abs(y_curr - y_next);
    
            Vector2 dir = (hilbertPoints[i + 1] - hilbertPoints[i]).normalized;
    
            if(x_dist == 0 && y_dist > 0)
            {
                for (int y = 0; y < y_dist; y++)
                {
                    if (x_curr >= 0 && x_curr < Mathf.Max(width, height) && 
                        y_curr + ((int)dir.y * y) >=0 && y_curr + ((int)dir.y * y) < Mathf.Max(width, height))
                    {
                        hilbertPointsInt[x_curr, y_curr + ((int)dir.y * y)] = 1;
                    }
                }
            }
            else if(x_dist > 0 && y_dist == 0)
            {
                for (int x = 0; x < x_dist; x++)
                {
                    if (x_curr + ((int)dir.x * x) >=0 && x_curr + ((int)dir.x * x) < Mathf.Max(width, height) &&
                        y_curr >= 0 && y_curr < Mathf.Max(width, height))
                    {
                        hilbertPointsInt[x_curr + ((int)dir.x * x), y_curr] = 1;
                    }
                }
            }
        }

        CreateNegativePath();
    }


    void FloodFillNeighboursWithValue(int x, int y, int val) {
        for (int i = -pathGirth; i <= pathGirth; i++) {
            for (int j = -pathGirth; j <= pathGirth; j++) {
                int i_x = i + x;
                int j_y = j + y;
    
                if (i_x < 0 || j_y < 0 || i_x > width - 1 || j_y > height - 1)
                    continue;   //skip if outside of grid's bounds
    
                generatedMap[i_x, j_y] = val;
            }
        }
    }


    void CreateNegativePath() {
        // creating 'negative' curve map
        for (int x = 1; x < width - 1; x++) {
            for (int y = 1; y < height - 1; y++) {
                hilbertNegativePointsInt[x, y] = (hilbertPointsInt[x, y] == 1) ? 0 : 1;
            }
        }
    
        // trim 'negative' curve map
        for (int x = 1; x < width - 1; x++) {
            for (int y = 1; y < height - 1; y++) {
                int neighbors = GetNeighboursNegativePathCellCount(x, y);
                hilbertNegativePointsInt[x, y] = (neighbors > 0) ? 0 : hilbertNegativePointsInt[x, y];
            }
        }
    }


    int GetNeighboursNegativePathCellCount(int x, int y)
    {
        int neighbors = 0;
        for (int i = -negativePathGirth; i <= negativePathGirth; i++)
        {
            for (int j = -negativePathGirth; j <= negativePathGirth; j++)
            {
                int i_x = i + x;
                int j_y = j + y;
    
                if (i_x <= 0 || j_y <= 0 || i_x > width - 1 || j_y > height - 1)
                    neighbors += 1;
                else
                    neighbors += hilbertPointsInt[i_x, j_y];
            }
        }
    
        neighbors -= hilbertPointsInt[x, y];
    
        return neighbors;
    }



    // Translates 1/0s of generatedMap to CellData and Tiles
    // CREDIT: Code adapted from Unity Learn
    void DrawMap() {
        boardData = new CellData[width,height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Tile tile;
                boardData[x,y] = new CellData();

                if (generatedMap[x,y] == 1) {
                    boardData[x,y].Passable = true;
                    tile = groundTiles[Random.Range(0, groundTiles.Length)];    //random ground tile art
                } else {
                    boardData[x,y].Passable = false;
                    tile = wallTiles[Random.Range(0, wallTiles.Length)];    //random wall tile art
                }

                tileMap.SetTile(new Vector3Int(x,y,0), tile);
            }
        }
    }


    // Translates a cell coordinate to a world coordinate
    // CREDIT: Code adapted from Unity Learn
    public Vector3 CellToWorld(Vector2Int cellIndex) {
        return grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }


    // Finds first (bottom-left) tile that is passable
    // CREDIT: I wrote this!
    Vector2Int FindFirstPassable()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (boardData[x, y].Passable) { return new Vector2Int(x, y); }
            }
        }
        Debug.Log("Impassable board!");
        return new Vector2Int(-1, -1);
    }


    // Places food objects on the board
    // CREDIT: Code adapted from Unity Learn
    void GenerateFood()
    {
        for (int i = 0; i < foodCount; i++)
        {
            int randomX = Random.Range(1, width - 1);
            int randomY = Random.Range(1, height - 1);
            CellData data = boardData[randomX, randomY];
            if (data.Passable && data.ContainedObject == null)
            {
                GameObject newFood = Instantiate(foodPrefab);
                newFood.transform.position = CellToWorld(new Vector2Int(randomX, randomY));
                data.ContainedObject = newFood;
            }
        }
    }
    

    // Places weapon objects on the board
    // CREDIT: Code adapted from Unity Learn
    void GenerateWeapons()
    {
        for (int i = 0; i < weaponCount; i++)
        {
            int randomX = Random.Range(1, width - 1);
            int randomY = Random.Range(1, height - 1);
            CellData data = boardData[randomX, randomY];
            if (data.Passable && data.ContainedObject == null)
            {
                GameObject newWeapon = Instantiate( weaponPrefabs[ Random.Range( 0, weaponPrefabs.Length-1 ) ] );
                newWeapon.transform.position = CellToWorld(new Vector2Int(randomX, randomY));
                data.ContainedObject = newWeapon;
            }
        }
    }



    // In-editor visualization
    void OnDrawGizmos() {
        CellularAutomata();
        
        if (generatedMap != null) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Gizmos.color = (generatedMap[x, y] == 1) ? Color.white : Color.black;
                    Vector3 pos = new Vector3(x + .5f, y + .5f, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    
        if (isDisplayGuidelines) {
            if (hilbertPointsInt != null) {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        Gizmos.color = (hilbertPointsInt[x, y] == 1) ? Color.cyan : Color.clear;
                        Vector3 pos = new Vector3(x + .5f, y + .5f, 0);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }

            if (hilbertNegativePointsInt != null) {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        Gizmos.color = (hilbertNegativePointsInt[x, y] == 1) ? Color.green : Color.clear;
                        Vector3 pos = new Vector3(x + .5f, y + .5f, 0);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }

            if (hilbertPoints != null) {
                for (int i = 0; i < hilbertPoints.Length - 1; i++) {
                    if (hilbertPoints[i].Equals(Vector2.zero) ||
                        hilbertPoints[i + 1].Equals(Vector2.zero))
                        continue;
        
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(new Vector3(hilbertPoints[i].x + .5f, hilbertPoints[i].y + .5f, 0),
                                    new Vector3(hilbertPoints[i + 1].x + .5f, hilbertPoints[i + 1].y + .5f, 0));
                }
            }
        }
    }


}
