using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkObject
{
    [SerializeField] private float syncRate, speed, jumpSpeed, sprintSpeed;
    public bool isLocalPlayer;
    private Rigidbody rb;
    private float elapsed = 0;
    private InputReceiver inputReceiver;
    private Vector2 movementInputVector => inputReceiver.movementInputVector;
    private bool sprint => inputReceiver.sprint;
    private bool jump => inputReceiver.jumpPress;
    private void Awake()
    {
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
    }
    public void ReceivePosition(Vector3 _position)
    {
        transform.position = _position;
        if (TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.SyncCamera();
        }
    }
    private void Update()
    {
        if (elapsed >= syncRate)
        {
            RequestPostion();
            elapsed = 0;
        }
        elapsed++;
        if (Client.ins.isHost && !isLocalPlayer) Move();

    }
    private void RequestPostion()
    {
        if (Client.ins.isHost && id == "1")
        {
            var pos = transform.position;
            var movePacket = new MovePlayerPacket();
            movePacket.WriteData(id, pos);
            Client.ins.SendUDPMessage(movePacket.GetString());
        }
    }
    private void Move()
    {
        if (movementInputVector != Vector2.zero)
        {
            var yVel = jump ? jumpSpeed : 0;
            var currentSpeed = sprint ? sprintSpeed : speed;
            rb.velocity = new Vector3(movementInputVector.x, 0, movementInputVector.y) * currentSpeed + Vector3.up * (rb.velocity.y + yVel);
            if (jump) Debug.Log("jump");
        }
        else
        {
            rb.velocity = Vector3.up * rb.velocity.y;
        }
    }
}
