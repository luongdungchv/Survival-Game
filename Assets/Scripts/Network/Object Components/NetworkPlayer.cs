using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class NetworkPlayer : NetworkObject
{
    [SerializeField] private float speed, jumpSpeed, sprintSpeed;
    public static NetworkPlayer localPlayer;
    public float syncRate, frameLerp;
    public bool isLocalPlayer;
    public int port;
    private float elapsed = 0;
    private bool isGrounded;
    private float timeTest;
    private Coroutine lerpPosRoutine, rotationCoroutine;
    private Vector3 lastPosition;
    public Queue<MovePlayerPacket> pendingStatePackets;

    private Rigidbody rb;
    private InputReceiver inputReceiver;
    private NetworkMovement netMovement;
    private StateMachine fsm;
    private Vector3 desiredPosition;
    private PlayerStats stats;

    private Vector2 movementInputVector => inputReceiver.movementInputVector;
    private bool sprint => inputReceiver.sprint;
    private bool jump => inputReceiver.jumpPress;
    private Vector2 camDir => inputReceiver.camDir;
    private float moveSpeed => stats.speed;
    private void Awake()
    {
        if (isLocalPlayer) localPlayer = this;

        fsm = GetComponent<StateMachine>();
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
        netMovement = GetComponent<NetworkMovement>();
        stats = GetComponent<PlayerStats>();
        
        lastPosition = transform.position;
        timeTest = Time.realtimeSinceStartup;
        pendingStatePackets = new Queue<MovePlayerPacket>();

    }
    private void FixedUpdate()
    {
        BroadcastState();
    }
    public void ReceivePlayerState(MovePlayerPacket packet)
    {
        var _position = packet.position;
        var moveDir = _position - lastPosition;

        if (moveDir.magnitude < 0.006) moveDir = Vector3.zero;

        if (moveDir != Vector3.zero)
        {
            moveDir.y = 0;

            if (moveDir.magnitude > 0.01)
            {
                float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
                float forwardAngle = -Mathf.Atan2(transform.forward.z, transform.forward.x) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle - forwardAngle) > 8)
                {
                    Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
                    rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
                }

            }
            moveDir = moveDir.normalized;
            //rb.MovePosition(_position);
            if (lerpPosRoutine != null) StopCoroutine(lerpPosRoutine);
            lerpPosRoutine = StartCoroutine(LerpPosition(_position, 0.0833f));
            this.desiredPosition = _position;
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
    public void HandlePlayerState(MovePlayerPacket packet)
    {
        if (!isLocalPlayer)
        {
            ReceivePlayerState(packet);
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

    private void BroadcastState()
    {
        var pos = rb.position;
        var movePacket = new MovePlayerPacket();
        movePacket.WriteData(id, pos, AnimationMapper.GetAnimationIndex(fsm.currentState));
        Client.ins.SendUDPPacket(movePacket);
    } 

}
