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
            GameManager.instance.SetItem(null);
            print("Item used");
        }

        //switch weapon
        if (switchAct.WasPressedThisFrame()) {
            GameManager.instance.SwitchWeapon();
            print("Weapon switched");
        }


        //if possible, actually move and tell enemies to move
        if (hasMoved) {
            if (board.boardData[ newCellTarget.x, newCellTarget.y ].Passable) {
                MoveTo(newCellTarget);
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


    void OnTriggerEnter2D(Collider2D coll) {
        print("triggered"); ////
        if (coll.gameObject.tag == "Food") {
            GameManager.instance.SetItem(coll.gameObject.GetComponent<Food>());
        }
        else if (coll.gameObject.tag == "Weapon") {
            GameManager.instance.SetWeapon1(coll.gameObject.GetComponent<Weapon>());
        }
        else if (coll.gameObject.tag == "Enemy") {
            Enemy e = coll.GetComponent<Enemy>();

            if (hasMoved && GameManager.instance.weapon1 != null) {     //if player moved, hurt enemy
                e.HPDown(GameManager.instance.weapon1.damage);
                MoveTo(prevCellPos);    //player does not move into space
            }
            else {      //if enemy moved into player, hurt player
                GameManager.instance.HPDown(e.damage);
                e.MoveToPrev();  //enemy does not move into space
            }
        }
        else if (coll.gameObject.tag == "Finish") {
            ////GameManager.instance.WinLevel();
        }
    }
}
