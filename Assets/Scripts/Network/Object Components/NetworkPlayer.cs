using System.Collections;
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
    private void Awake()
    {
        if (isLocalPlayer) localPlayer = this;

        fsm = GetComponent<StateMachine>();
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
        netMovement = GetComponent<NetworkMovement>();
        lastPosition = transform.position;
    }
    private void Update()
    {
        RequestPostion();
        //if (Client.ins.isHost && !isLocalPlayer) netMovement.MoveServer();

    }
    public void ReceivePlayerState(MovePlayerPacket packet)
    {
        var _position = packet.position;
        var moveDir = _position - lastPosition;

        if (moveDir.magnitude < 0.01) moveDir = Vector3.zero;
        if (moveDir != Vector3.zero && id == "1")
        {
            Debug.Log($"{_position} {moveDir.magnitude}");
        }

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
            //StartCoroutine(LerpPosition(_position));
            rb.MovePosition(_position);

        }
        else
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        }
        if (TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.SyncCamera();
        }
        lastPosition = _position;
        fsm.ChangeState(AnimationMapper.GetAnimationName(packet.anim));
    }
    private IEnumerator LerpPosition(Vector3 to)
    {
        float t = 0;
        Vector3 from = rb.position;
        while (t < 1)
        {
            rb.position = Vector3.Lerp(from, to, t);
            t += Time.deltaTime / frameLerp;
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

    private void RequestPostion()
    {
        if (Client.ins.isHost)
        {
            var pos = transform.position;
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
