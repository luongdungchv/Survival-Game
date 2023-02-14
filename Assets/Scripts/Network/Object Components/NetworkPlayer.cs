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
    private float elapsed = 0;
    private bool isGrounded;
    private float timeTest;
    private Coroutine lerpPosRoutine, rotationCoroutine;
    private Vector3 lastPosition;
    public Queue<MovePlayerPacket> pendingStatePackets;
    public StatePayload[] stateBuffer = new StatePayload[1024];

    private Rigidbody rb;
    private InputReceiver inputReceiver;
    private NetworkMovement netMovement;
    private StateMachine fsm;

    private Vector3 lastServerPos;
    private Vector3 lastClientPos;

    private Vector2 movementInputVector => inputReceiver.movementInputVector;
    private bool sprint => inputReceiver.sprint;
    private bool jump => inputReceiver.jumpPress;
    private Vector2 camDir => inputReceiver.camDir;
    private void Awake()
    {
        if (isLocalPlayer) localPlayer = this;
        stateBuffer = new StatePayload[1024];

        fsm = GetComponent<StateMachine>();
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
        netMovement = GetComponent<NetworkMovement>();
        lastPosition = transform.position;
        timeTest = Time.realtimeSinceStartup;
        pendingStatePackets = new Queue<MovePlayerPacket>();

        lastServerPos = transform.position;
        lastClientPos = transform.position;
    }
    private void FixedUpdate()
    {
        BroadcastState();
        if (!Client.ins.isHost && pendingStatePackets.Count > 0)
        {
            var packet = pendingStatePackets.Dequeue();
            //Debug.Log(packet.position);
            // ReceivePlayerState(packet);
            //HandlePlayerState(packet);
        }
    }
    private void Update()
    {


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
    private int lastServerTick;
    public void HandlePlayerState(MovePlayerPacket packet)
    {
        if (!isLocalPlayer)
        {
            ReceivePlayerState(packet);
        }
        else
        {

            if (packet.tick >= InputReader.ins.currentTick) return;
            var serverPosition = packet.position;
            var clientPosition = stateBuffer[packet.tick].position;
            var diff = Vector3.Distance(serverPosition, clientPosition);

            var serverDir = serverPosition - lastServerPos;
            var clientDir = clientPosition - lastClientPos;
            var diffAngle = Vector3.Angle(serverDir, clientDir);

            if (diff >= 0.5f)
            {             
                if (packet.tick - lastServerTick > 1)
                {          
                    Debug.Log($"{packet.tick} {lastServerTick}");
                }
                Debug.Log($"reconciliation: {diff} {serverPosition} {clientPosition} {InputReader.ins.currentTick} {packet.tick}");
                HandleReconciliation(packet.tick, packet);
            }

            if (diff >= 3f && diffAngle > 8 && clientPosition != Vector3.zero)
            {


            }
            lastServerTick = packet.tick;
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
        if (Client.ins.isHost)
        {
            var pos = rb.position;
            var movePacket = new MovePlayerPacket();
            movePacket.WriteData(id, pos, AnimationMapper.GetAnimationIndex(fsm.currentState));
            movePacket.tick = inputReceiver.tick;
            Client.ins.SendUDPPacket(movePacket);
            //Client.ins.SendUDPMessage(movePacket.GetString());
        }
    }
    private void HandleReconciliation(int bufferIndex, MovePlayerPacket correctPacket)
    {
        var correctedState = new StatePayload(correctPacket.position, correctPacket.tick);
        InputReader.ins.currentTick = correctedState.tick;
        rb.position = correctedState.position;

    }
    public void AddPacket(MovePlayerPacket packet)
    {
        pendingStatePackets.Enqueue(packet);
    }
    public void AddStatePayload(StatePayload payload)
    {
        if (payload.tick < 0) return;
        stateBuffer[payload.tick] = payload;
        //Debug.Log(payload.position + payload.tick.ToString());
    }
}
public struct StatePayload
{
    public Vector3 position;
    public int tick;
    public StatePayload(Vector3 position, int tick)
    {
        this.position = position;
        this.tick = tick;

    }
    public StatePayload(MovePlayerPacket packet) : this(packet.position, packet.tick)
    {
    }

}
