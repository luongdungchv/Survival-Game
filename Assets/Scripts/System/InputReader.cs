using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputReader : MonoBehaviour
{
    public static InputReader ins;
    public event UnityAction OnSlashPress;
    public event UnityAction OnSprintPress;

    public KeyCode moveForwardKey, moveBackWardKey, moveLeftKey, moveRightKey,
        slashKey, sprintKey, showCursorKey, jumpKey, collectBtn;


    public bool sprint;

    public Vector2 movementInputVector;
    private Vector2[] moveDirections = new Vector2[4];
    void Start()
    {
        ins = this;
        //showCursorKey = KeyCode.LeftAlt
    }

    // Update is called once per frame
    void Update()
    {

        if (UIManager.ins.isMapOpen) return;
        moveDirections[0] = GetKey(moveForwardKey) ? Vector2.up : Vector2.zero;
        moveDirections[1] = GetKey(moveBackWardKey) ? Vector2.down : Vector2.zero;
        moveDirections[2] = GetKey(moveRightKey) ? Vector2.right : Vector2.zero;
        moveDirections[3] = GetKey(moveLeftKey) ? Vector2.left : Vector2.zero;
        movementInputVector = Vector2.zero;
        foreach (var i in moveDirections)
        {
            movementInputVector += i;
        }
        //Debug.Log(movementInputVector);
        if (SprintPress()) sprint = true;
        if (SprintRelease()) sprint = false;

    }
    private bool GetKey(KeyCode key)
    {
        return Input.GetKey(key) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool SlashPress()
    {
        return Input.GetKeyDown(slashKey) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool SlashRelease()
    {
        return Input.GetKeyUp(slashKey) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool SprintPress()
    {
        return (Input.GetKeyDown(sprintKey) || Input.GetMouseButtonDown(1)) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool SprintRelease()
    {
        return (Input.GetKeyUp(sprintKey) || Input.GetMouseButtonUp(1)) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool ShowCursorKeyPress()
    {
        return Input.GetKeyDown(showCursorKey);
    }
    public bool ShowCursorKeyRelease()
    {
        return Input.GetKeyUp(showCursorKey);
    }
    public bool JumpPress()
    {
        return Input.GetKeyDown(jumpKey) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool JumpRelease()
    {
        return Input.GetKeyUp(jumpKey) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public bool InteractBtnPress()
    {
        return Input.GetKeyDown(collectBtn) && !UIManager.ins.isMapOpen && !Cursor.visible;
    }
    public float MouseX()
    {
        return UIManager.ins.isMapOpen || Cursor.visible ? 0 : Input.GetAxis("Mouse X");
    }
    public float MouseY()
    {
        return UIManager.ins.isMapOpen || Cursor.visible ? 0 : Input.GetAxis("Mouse Y");
    }


}
