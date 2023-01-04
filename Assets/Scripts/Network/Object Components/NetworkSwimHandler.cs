using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public class NetworkSwimHandler : MonoBehaviour
{
    [SerializeField]
    private float swimSpeed, swimFastSpeed;
    [SerializeField] private Transform castPos;
    [SerializeField] private float rayLength, threshold;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private State SwimNormal, SwimIdle;
    private RaycastHit hit;
    private StateMachine fsm;
    //private bool isSwimming => netFSM.currentState == NetworkPlayerState.Swimming || netFSM.currentState == NetworkPlayerState.SwimIdle;
    private bool isSwimming;
    private float currentSpeed, lastCurrentSpeed;

    private Rigidbody rb => GetComponent<Rigidbody>();
    private InputReceiver inputReceiver => GetComponent<InputReceiver>();
    // Start is called before the first frame update
    void Start()
    {
        fsm = GetComponent<StateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(castPos.position, Vector3.up, out hit, rayLength, rayMask))
        {
            //Debug.Log(hit.point);
            Debug.DrawLine(castPos.position, hit.point, new Color(1, 0, 1));
            var length = Vector3.Distance(hit.point, castPos.position);
            if (length >= threshold && !isSwimming)
            {
                //Debug.Log($"start swim, start: {castPos.position} || end: {hit.point}");
                //transform.position = new Vector3(transform.position.x, hit.point.y - threshold - 0.5f, transform.position.z);
                var rb = GetComponent<Rigidbody>();
                Debug.Log("start swim");
                rb.useGravity = false;
                GetComponent<NetworkStateManager>().InAir.lockState = false;
                GetComponent<PlayerAnimation>().CancelJump();
                rb.velocity = new Vector3(0, 0, 0);
                isSwimming = true;

                Debug.Log(SwimNormal.SetLock("Move", true));
                SwimNormal.SetLock("Idle", true);

                SwimIdle.SetLock("Move", true);
                SwimIdle.SetLock("Idle", true);

                fsm.ChangeState(SwimIdle);
            }
            if (length < threshold - 0.2f && isSwimming)
            {
                Debug.Log($"reach edge, start: {castPos.position} || end: {hit.point}");

                SwimNormal.SetLock("Move", false);
                SwimNormal.SetLock("Idle", false);
                Debug.Log(SwimNormal.CheckLock("Idle"));
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
                //animSystem.SwimNormal();
                currentSpeed = swimFastSpeed;
            }
            if (!inputReceiver.sprint && swimSpeed != currentSpeed)
            {
                //animSystem.SwimNormal();
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
            // if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);

            // Vector3 camForward = new Vector3(camHolder.forward.x, 0, camHolder.forward.z).normalized;
            // Vector3 camRight = new Vector3(camHolder.right.x, 0, camHolder.right.z).normalized;

            // moveDir = (camForward * zMove + camRight * xMove).normalized * currentSpeed;

            // float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;

            // Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            //     angle + 90, transform.rotation.eulerAngles.z);

            // rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
            //Debug.Log(targetRotation.eulerAngles);
            //transform.rotation = targetRotation;
        }
        else
        {
            fsm.ChangeState(SwimIdle);
        }

        rb.velocity = new Vector3(moveDir.x, 0, moveDir.z);
    }
    public void StopSwimServer()
    {
        rb.velocity = Vector3.zero;
    }
}
