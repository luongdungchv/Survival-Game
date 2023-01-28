﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NetworkPlayer : NetworkObject
{
    [SerializeField] private float speed, jumpSpeed, sprintSpeed;
    public static NetworkPlayer localPlayer;
    public float syncRate, frameLerp;
    public bool isLocalPlayer;
    public int port;
    private Rigidbody rb;
    private float elapsed = 0;
    private InputReceiver inputReceiver;
    private NetworkMovement netMovement;
    private Vector2 movementInputVector => inputReceiver.movementInputVector;
    private bool sprint => inputReceiver.sprint;
    private bool jump => inputReceiver.jumpPress;
    private Vector2 camDir => inputReceiver.camDir;
    private bool isGrounded;
    private StateMachine fsm;
    private Coroutine lerpPosRoutine, rotationCoroutine;
    private Vector3 lastPosition;
    private float timeTest;
    private void Awake()
    {
        if (isLocalPlayer) localPlayer = this;

        fsm = GetComponent<StateMachine>();
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
        netMovement = GetComponent<NetworkMovement>();
        lastPosition = transform.position;
        timeTest = Time.realtimeSinceStartup;
    }
    private void Update()
    {
        RequestState();
    }
    public void ReceivePlayerState(MovePlayerPacket packet)
    {
        var _position = packet.position;
        var moveDir = _position - lastPosition;
        // Debug.Log((Time.realtimeSinceStartup - timeTest) * 1000);
        // timeTest = Time.realtimeSinceStartup;

        if (moveDir.magnitude < 0.006) moveDir = Vector3.zero;

        if (moveDir != Vector3.zero)
        {
            moveDir.y = 0;

            if (moveDir.magnitude > 0.01)
            {
                float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
                rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));

            }
            moveDir = moveDir.normalized;
            //rb.MovePosition(_position);
            if (lerpPosRoutine != null) StopCoroutine(lerpPosRoutine);
            lerpPosRoutine = StartCoroutine(LerpPosition(_position, 0.09f));

        }
        else
        {
            rb.velocity = Vector3.zero;
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        }
        if (TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.SyncCamera();
        }
        lastPosition = _position;
        if (!isLocalPlayer)
        {
            if (packet.anim == 7) inputReceiver.attack = true;
            else
            {
                inputReceiver.attack = false;

            }
            fsm.ChangeState(AnimationMapper.GetAnimationName(packet.anim));
        }
    }
    private IEnumerator LerpPosition(Vector3 to, float duration)
    {
        Vector3 from = rb.position;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            rb.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }
    IEnumerator LerpRotation(Quaternion from, Quaternion to, float duration)
    {
        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Lerp(from, to, t);
            yield return null;
        }
    }

    private void RequestState()
    {
        if (Client.ins.isHost)
        {
            var pos = rb.position;
            var movePacket = new MovePlayerPacket();
            movePacket.WriteData(id, pos, AnimationMapper.GetAnimationIndex(fsm.currentState));
            Client.ins.SendUDPPacket(movePacket);
            //Client.ins.SendUDPMessage(movePacket.GetString());
        }
    }
    private void Move()
    {
        if (movementInputVector != Vector2.zero)
        {
            var currentSpeed = sprint ? sprintSpeed : speed;
            var moveDir = new Vector3(camDir.x, 0, camDir.y) * movementInputVector.y
                        + new Vector3(camDir.y, 0, -camDir.x) * movementInputVector.x;
            moveDir = moveDir.normalized;
            Debug.Log(moveDir * currentSpeed + Vector3.up * rb.velocity.y);

            rb.velocity = moveDir * currentSpeed + Vector3.up * rb.velocity.y;

            float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
            rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
        }
        if (jump)
        {
            rb.velocity = Vector3.up * jumpSpeed;
        }

    }

}
