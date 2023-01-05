using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputReader : MonoBehaviour
{
    public static InputReader ins;


    public KeyCode moveForwardKey, moveBackWardKey, moveLeftKey, moveRightKey,
        slashKey, sprintKey, showCursorKey, jumpKey, interactKey, openMapKey,
        openInventoryKey;


    public bool sprint;

    public Vector2 movementInputVector;
    private Vector2[] moveDirections = new Vector2[4];
    [SerializeField] private float readDelay;
    [SerializeField] private Transform localCamHolder;
    private float elapsed;

    private void Awake()
    {
        ins = this;
    }
    void Start()
    {

        //showCursorKey = KeyCode.LeftAlt
        // Client.ins.OnUDPMessageReceive.AddListener((msg) =>
        // {
        //     var split = msg.Split(' ');
        //     if (split[0] == "input" && split[1] == Client.ins.clientId)
        //     {
        //         movementInputVector = new Vector2(float.Parse(split[2]), float.Parse(split[3]));
        //         if (movementInputVector != Vector2.zero)
        //             Debug.Log(msg);
        //     }
        // });
    }
    public int inputNum;

    // Update is called once per frame
    void Update()
    {

        moveDirections[0] = GetKey(moveForwardKey) ? Vector2.up : Vector2.zero;
        moveDirections[1] = GetKey(moveBackWardKey) ? Vector2.down : Vector2.zero;
        moveDirections[2] = GetKey(moveRightKey) ? Vector2.right : Vector2.zero;
        moveDirections[3] = GetKey(moveLeftKey) ? Vector2.left : Vector2.zero;
        var tmpMoveVector = Vector2.zero;
        foreach (var i in moveDirections)
        {
            tmpMoveVector += i;
        }
        if (Input.inputString != null && Input.inputString.Length > 0)
        {
            foreach (var i in Input.inputString)
            {
                if (Char.IsDigit(i))
                {
                    inputNum = int.Parse(i.ToString());
                    Debug.Log(inputNum);
                    break;
                }
            }
        }
        else inputNum = -1;
        //Debug.Log(movementInputVector);
        if (SprintPress()) sprint = true;
        if (SprintRelease()) sprint = false;

        if (elapsed >= readDelay || JumpPress() || SlashPress())
        {
            if (Client.ins.isHost)
            {
                movementInputVector = tmpMoveVector;
            }
            else
            {
                movementInputVector = tmpMoveVector;
                var inputPacket = new InputPacket();
                var camDir = new Vector2(localCamHolder.forward.x, localCamHolder.forward.z).normalized;
                inputPacket.WriteData(Client.ins.clientId, tmpMoveVector, sprint, JumpPress(), camDir, SlashPress());
                //Client.ins.SendUDPMessage(inputPacket.GetString());
                Client.ins.SendUDPPacket(inputPacket);
            }
            elapsed = 0;
        }
        elapsed += Time.deltaTime;
    }
    private bool GetKey(KeyCode key)
    {
        return Input.GetKey(key) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool SlashPress()
    {
        return Input.GetMouseButtonDown(0) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool SlashRelease()
    {
        return Input.GetKeyUp(slashKey) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool SprintPress()
    {
        return (Input.GetKeyDown(sprintKey) || Input.GetMouseButtonDown(1)) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool SprintRelease()
    {
        return (Input.GetKeyUp(sprintKey) || Input.GetMouseButtonUp(1)) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool ShowCursorKeyPress()
    {
        return Input.GetKeyDown(showCursorKey) && !UIManager.ins.isUIOpen;
    }
    public bool ShowCursorKeyRelease()
    {
        return Input.GetKeyUp(showCursorKey) && !UIManager.ins.isUIOpen;
    }
    public bool JumpPress()
    {
        return Input.GetKeyDown(jumpKey) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool JumpRelease()
    {
        return Input.GetKeyUp(jumpKey) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool InteractBtnPress()
    {
        return Input.GetKeyDown(interactKey) && !UIManager.ins.isUIOpen && !Cursor.visible;
    }
    public bool OpenMapPress()
    {
        return Input.GetKeyDown(openMapKey);
    }
    public bool OpenInventoryPress()
    {
        return Input.GetKeyDown(openInventoryKey);
    }
    public float MouseX()
    {
        return UIManager.ins.isUIOpen || Cursor.visible ? 0 : Input.GetAxis("Mouse X");
    }
    public float MouseY()
    {
        return UIManager.ins.isUIOpen || Cursor.visible ? 0 : Input.GetAxis("Mouse Y");
    }
    public int MouseScroll()
    {
        return UIManager.ins.isUIOpen || Cursor.visible ? 0 : (int)Input.mouseScrollDelta.y;
    }

}
