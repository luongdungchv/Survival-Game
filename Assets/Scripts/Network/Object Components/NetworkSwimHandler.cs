using UnityEngine;
using System.Collections;

public class NetworkSwimHandler : MonoBehaviour
{

    [SerializeField] private float rayLength, threshold;
    [SerializeField] private Transform castPos;
    [SerializeField] private LayerMask rayMask;

    private RaycastHit hit;
    private bool isSwimming;
    private float currentSpeed, lastCurrentSpeed;
    private Coroutine rotationCoroutine;

    private NetworkStateManager stateManager;
    private StateMachine fsm;
    private Rigidbody rb;
    private InputReceiver inputReceiver;
    private PlayerStats stats;

    private State SwimNormal => stateManager.SwimNormal;
    private State SwimIdle => stateManager.SwimIdle;
    private float swimSpeed => stats.swimSpeed;
    private float swimFastSpeed => stats.swimFastSpeed;
    void Awake()
    {
        fsm = GetComponent<StateMachine>();
        stateManager = GetComponent<NetworkStateManager>();
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Physics.Raycast(castPos.position, Vector3.up, out hit, rayLength, rayMask))
        {
            Debug.DrawLine(castPos.position, hit.point, new Color(1, 0, 1));
            var length = Vector3.Distance(hit.point, castPos.position);
            if (length >= threshold && !isSwimming)
            {
                var rb = GetComponent<Rigidbody>();
                rb.useGravity = false;
                GetComponent<NetworkStateManager>().InAir.lockState = false;
                GetComponent<PlayerAnimation>().CancelJump();
                rb.velocity = new Vector3(0, 0, 0);
                isSwimming = true;

                SwimNormal.SetLock("Idle", true);
                SwimNormal.SetLock("Move", true);
                SwimIdle.SetLock("Move", true);
                SwimIdle.SetLock("Idle", true);

                fsm.ChangeState(SwimIdle);
            }
            if (length < threshold - 0.2f && isSwimming)
            {
                Debug.Log($"reach edge, start: {castPos.position} || end: {hit.point}");

                SwimNormal.SetLock("Move", false);
                SwimNormal.SetLock("Idle", false);
                SwimIdle.SetLock("Move", false);
                SwimIdle.SetLock("Idle", false);

                fsm.ChangeState("Idle");
                rb.useGravity = true;

                isSwimming = false;
            }
        }
    }
    public void SwimDetect()
    {
        if (inputReceiver.movementInputVector != Vector2.zero)
        {
            fsm.ChangeState(SwimNormal);
        }
    }
    public void SwimServer()
    {
        float xMove = inputReceiver.movementInputVector.x;
        float zMove = inputReceiver.movementInputVector.y;

        Vector3 moveDir = Vector3.zero;

        if (xMove != 0 || zMove != 0)
        {
            if (inputReceiver.sprint && swimFastSpeed != currentSpeed)
            {
                currentSpeed = swimFastSpeed;
            }
            if (!inputReceiver.sprint && swimSpeed != currentSpeed)
            {
                currentSpeed = swimSpeed;
            }
            if (currentSpeed != lastCurrentSpeed)
            {
                lastCurrentSpeed = currentSpeed;
            }
            var camDir = inputReceiver.camDir;
            moveDir = new Vector3(camDir.x, 0, camDir.y) * zMove
                        + new Vector3(camDir.y, 0, -camDir.x) * xMove;
            moveDir = moveDir.normalized * currentSpeed;
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);

            float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
            rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
        }
        else
        {
            fsm.ChangeState(SwimIdle);
        }

        rb.velocity = new Vector3(moveDir.x, 0, moveDir.z);
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

    public void StopSwimServer()
    {
        rb.velocity = Vector3.zero;
    }
}
