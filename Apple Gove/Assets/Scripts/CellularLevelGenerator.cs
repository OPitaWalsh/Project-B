using UnityEngine;

// Code by Pav, https://pavcreations.com/procedural-generation-of-2d-maps-in-unity/3/
// Comments by me to improve and prove my understanding

public class CellularLevelGenerator : MonoBehaviour
{
    int[,] generatedMap;    //2D array of grid
    public int width;
    public int height;

    public string seed;
    private System.Random pseudoRandom;

    [Range(0,100)]
    public int fillingPercentage;



    void Start()
    {
        CellularAutomata();
    }



    void CellularAutomata() {
        //if no seed, create a seed from the clock
        seed = (seed.Length <= 0) ? Time.time.ToString() : seed;
        //generate a PRNG
        pseudoRandom = new System.Random(seed.GetHashCode());

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


    // Ignoring the edges, if a cell has >4 filled neighbors, fill it; <4, empty it; exactly 4, keep as is
    void SmoothMap() {
        for (int x = 1; x < width-1; x++) {
            for (int y = 1; y < height-1; y++) {
                int neighbors = GetNeighborsCellCount(x,y,generatedMap);

                if (neighbors > 4) { generatedMap[x,y] = 1; }
                else if (neighbors < 4) { generatedMap[x,y] = 0; }
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
    }
}
