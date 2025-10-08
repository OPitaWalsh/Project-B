using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
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
        bool hasMoved = false;

        //movement
        if (moveAct.WasPressedThisFrame()) {
            Vector2 moveValue = moveAct.ReadValue<Vector2>();
            if (moveValue.x > deadzone) {
                print("Move right");
            } else if (moveValue.x < -deadzone) {
                print("Move left");
            } else if (moveValue.y > deadzone) {
                print("Move up");
            } else if (moveValue.y < -deadzone) {
                print("Move down");
            }
            hasMoved = true;
        }
        
        //use item
        if (itemAct.WasPressedThisFrame()) {
            GameManager.instance.SetItem(0);
            print("Item used");
        }

        //switch weapon
        if (switchAct.WasPressedThisFrame()) {
            GameManager.instance.SwitchWeapon();
            print("Weapon switched");
        }


        //tell enemies to move
        if (hasMoved) {
            GameManager.instance.MoveEnemies();
            print("Has moved");
        }
    }
}
