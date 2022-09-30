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
        if (Input.GetKeyDown(KeyCode.M))
        {
            UIManager.ins.ToggleMap();
        }
    }
    public bool SlashPress()
    {
        return Input.GetKeyDown(slashKey);
    }
    public bool SlashRelease()
    {
        return Input.GetKeyUp(slashKey);
    }
    public bool SprintPress()
    {
        return Input.GetKeyDown(sprintKey) || Input.GetMouseButtonDown(1);
    }
    public bool SprintRelease()
    {
        return Input.GetKeyUp(sprintKey) || Input.GetMouseButtonUp(1);
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
        return Input.GetKeyDown(jumpKey);
    }
    public bool jumpRelease()
    {
        return Input.GetKeyUp(jumpKey);
    }

}
