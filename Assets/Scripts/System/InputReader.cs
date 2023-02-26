﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    public static InputReader ins;


    public KeyCode moveForwardKey, moveBackWardKey, moveLeftKey, moveRightKey,
        slashKey, sprintKey, showCursorKey, jumpKey, interactKey, openMapKey,
        openInventoryKey;


    public bool sprint;
    public int currentTick = -1;

    public Vector2 movementInputVector;
    private Vector2[] moveDirections = new Vector2[4];
    [SerializeField] private float readDelay;
    [SerializeField] private Transform localCamHolder;
    private PlayerStats localPlayerStats;
    private PlayerEquipment localPlayerEquipment;
    
    private NetworkPlayer localPlayer => NetworkPlayer.localPlayer;
    private float elapsed;
    public Queue<InputPacket> inputBuffer;

    private void Awake()
    {
        ins = this;
        
        inputBuffer = new Queue<InputPacket>();
    }
    void Start()
    {
        localPlayerStats = NetworkPlayer.localPlayer.GetComponent<PlayerStats>();
        localPlayerEquipment = NetworkPlayer.localPlayer.GetComponent<PlayerEquipment>();
    }
    public int inputNum;

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
                    break;
                }
            }
        }
        else inputNum = -1;
        if (SprintPress()) sprint = true;
        if (SprintRelease()) sprint = false;

        if (elapsed >= readDelay || JumpPress() || SlashPress())
        {
            int numLoop = (int)(elapsed / readDelay);
            numLoop = numLoop == 0 ? 1 : numLoop;
            for (int i = 0; i < numLoop; i++)
            {
                currentTick++;
                currentTick = currentTick % 1024;
                var camDir = new Vector2(localCamHolder.forward.x, localCamHolder.forward.z).normalized;
                var inputPacket = new InputPacket(){
                        id = Client.ins.clientId,
                        inputVector = tmpMoveVector,
                        camDir = camDir,
                        sprint =  sprint && localPlayerStats.stamina > 0,
                        jump = JumpPress(),
                        atk = SlashPress(),
                        isConsumingItem = localPlayerEquipment.isConsumingItem,
                        tick = currentTick                       
                };
                inputBuffer.Enqueue(inputPacket);
            }
            elapsed -= ((int)(elapsed / readDelay) * readDelay);
        }
        elapsed += Time.deltaTime;
    }
    private void FixedUpdate() {
        if(inputBuffer.Count == 0) return;
        var inputPacket = inputBuffer.Dequeue();
        this.movementInputVector = inputPacket.inputVector;
        this.currentTick = inputPacket.tick;
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
