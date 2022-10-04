using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputReader : MonoBehaviour
{
    // Start is called before the first frame update

    public event UnityAction OnSlashPress;
    public event UnityAction OnSprintPress;

    public KeyCode moveForwardKey, moveBackWardKey, moveLeftKey, moveRightKey,
        slashKey, sprintKey, showCursorKey, jumpKey;


    public bool sprint;

    public Vector2 movementInputVector;
    private Vector2[] moveDirections = new Vector2[4];
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            UIManager.ins.ToggleMap();
        }
        if (UIManager.ins.isMapOpen) return;
        moveDirections[0] = Input.GetKey(moveForwardKey) ? Vector2.up : Vector2.zero;
        moveDirections[1] = Input.GetKey(moveBackWardKey) ? Vector2.down : Vector2.zero;
        moveDirections[2] = Input.GetKey(moveRightKey) ? Vector2.right : Vector2.zero;
        moveDirections[3] = Input.GetKey(moveLeftKey) ? Vector2.left : Vector2.zero;
        movementInputVector = Vector2.zero;
        foreach (var i in moveDirections)
        {
            movementInputVector += i;
        }
        //Debug.Log(movementInputVector);
        if (SprintPress()) sprint = true;
        if (SprintRelease()) sprint = false;

    }
    public bool SlashPress()
    {
        return Input.GetKeyDown(slashKey) && !UIManager.ins.isMapOpen;
    }
    public bool SlashRelease()
    {
        return Input.GetKeyUp(slashKey) && !UIManager.ins.isMapOpen;
    }
    public bool SprintPress()
    {
        return (Input.GetKeyDown(sprintKey) || Input.GetMouseButtonDown(1)) && !UIManager.ins.isMapOpen;
    }
    public bool SprintRelease()
    {
        return (Input.GetKeyUp(sprintKey) || Input.GetMouseButtonUp(1)) && !UIManager.ins.isMapOpen;
    }
    public bool ShowCursorKeyPress()
    {
        return Input.GetKeyDown(showCursorKey) && !UIManager.ins.isMapOpen;
    }
    public bool ShowCursorKeyRelease()
    {
        return Input.GetKeyUp(showCursorKey) && !UIManager.ins.isMapOpen;
    }
    public bool JumpPress()
    {
        return Input.GetKeyDown(jumpKey) && !UIManager.ins.isMapOpen;
    }
    public bool JumpRelease()
    {
        return Input.GetKeyUp(jumpKey) && !UIManager.ins.isMapOpen;
    }
    public float MouseX()
    {
        return UIManager.ins.isMapOpen ? 0 : Input.GetAxis("Mouse X");
    }
    public float MouseY()
    {
        return UIManager.ins.isMapOpen ? 0 : Input.GetAxis("Mouse Y");
    }

}
