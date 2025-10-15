using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    private BoardManager board;
    public Vector2Int cellPos;
    private Vector2Int prevCellPos;
    private bool hasMoved;

    private float deadzone;
    //InputActions
    private InputAction moveAct;
    private InputAction itemAct;
    private InputAction switchAct;


    void Start()
    {
        deadzone = GameManager.instance.deadzone;
        //inputs
        moveAct = InputSystem.actions.FindAction("Move");
        itemAct = InputSystem.actions.FindAction("UseItem");
        switchAct = InputSystem.actions.FindAction("SwitchWeapon");
    }


    void Update()
    {
        Vector2Int newCellTarget = cellPos;
        hasMoved = false;

        //get movement inputs
        if (moveAct.WasPressedThisFrame()) {
            Vector2 moveValue = moveAct.ReadValue<Vector2>();
            if (moveValue.x > deadzone) {
                newCellTarget.x += 1;
            } else if (moveValue.x < -deadzone) {
                newCellTarget.x -= 1;
            } else if (moveValue.y > deadzone) {
                newCellTarget.y += 1;
            } else if (moveValue.y < -deadzone) {
                newCellTarget.y -= 1;
            }
            hasMoved = true;
        }
        
        //use item
        if (itemAct.WasPressedThisFrame()) {
            GameManager.instance.UseItem();
        }

        //switch weapon
        if (switchAct.WasPressedThisFrame()) {
            GameManager.instance.SwitchWeapon();
        }


        //if possible, actually processes movementmove, colle and tell enemies to move
        if (hasMoved) {
            BoardManager.CellData newCell = board.boardData[newCellTarget.x, newCellTarget.y];
            //move
            if (newCell.Passable) {
                MoveTo(newCellTarget);

                //collect object or touch enemy
                GameObject newObj = newCell.ContainedObject;
                if (newObj != null) {
                    if (newObj.CompareTag("Food")) {
                        GameManager.instance.SetItem(newObj.GetComponent<Food>());
                        newObj.transform.position = new Vector3(0, 0, 0);       //hide item
                        newCell.ContainedObject = null;     //item cannot be recollected
                    }
                    else if (newObj.CompareTag("Weapon")) {
                        GameManager.instance.SetWeapon1(newObj.GetComponent<Weapon>());
                        newObj.transform.position = new Vector3(0, 0, 0);       //hide item
                        newCell.ContainedObject = null;     //item cannot be recollected
                    }
                    else if (newObj.CompareTag("Enemy")) {
                        TouchEnemy(newObj.GetComponent<Enemy>());
                    }
                    else if (newObj.CompareTag("Finish")) {
                        GameManager.instance.WinLevel();
                    }
                    
                }

                //tell enemies to move
                GameManager.instance.MoveEnemies();
            }
        }
    }
    


    public void Spawn(Vector2Int cell) {
        board = GameManager.instance.GetComponentInChildren<BoardManager>();
        MoveTo(cell);
    }


    public void MoveTo(Vector2Int cell) {
        prevCellPos = cellPos;
        cellPos = cell;
        transform.position = board.CellToWorld(cellPos);
    }


    public void TouchEnemy(Enemy e) {
        if (hasMoved && GameManager.instance.weapon1 != null) {     //if player moved, and has weapon, hurt enemy
            e.HPDown(GameManager.instance.weapon1.damage);
            MoveTo(prevCellPos);    //player moves back to previous position
        }
        else {      //if enemy moved into player, or if player has no weapon, hurt player
            GameManager.instance.HPDown(e.damage);
            e.MoveToPrev();  //enemy does not move into space
        }
    }
}
