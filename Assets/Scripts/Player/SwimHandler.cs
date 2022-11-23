using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimHandler : MonoBehaviour
{
    private RaycastHit hit;
    [SerializeField] private Transform castPos, camHolder;
    [SerializeField] private float rayLength, threshold, swimFastSpeed, swimSpeed;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private StateMachine fsm;
    [SerializeField] private InputReader inputReader;

    private Rigidbody rb;
    private Coroutine rotationCoroutine;
    private bool isStartSwim;
    private float currentSpeed, lastCurrentSpeed;
    private PlayerAnimation animSystem;
    private PlayerAttack attackSystem;
    // Start is called before the first frame update
    void Start()
    {

        fsm = GetComponent<StateMachine>();
        animSystem = GetComponent<PlayerAnimation>();
        rb = GetComponent<Rigidbody>();
        attackSystem = GetComponent<PlayerAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(castPos.position, Vector3.up, out hit, rayLength, rayMask))
        {
            //Debug.Log(hit.point);
            Debug.DrawLine(castPos.position, hit.point, new Color(1, 0, 1));
            var length = Vector3.Distance(hit.point, castPos.position);
            if (length >= threshold && !fsm.currentState.name.Contains("Swim"))
            {
                //Debug.Log($"start swim, start: {castPos.position} || end: {hit.point}");
                //transform.position = new Vector3(transform.position.x, hit.point.y - threshold - 0.5f, transform.position.z);
                var rb = GetComponent<Rigidbody>();
                rb.useGravity = false;
                GetComponent<StateInitializer>().InAir.lockState = false;
                rb.velocity = new Vector3(0, 0, 0);
                animSystem.CancelJump();
                fsm.ChangeState("SwimIdle");
            }
            if (length < threshold - 0.2f && fsm.currentState.name.Contains("Swim"))
            {
                Debug.Log($"reach edge, start: {castPos.position} || end: {hit.point}");
                fsm.ChangeState("Idle");
            }
        }
    }
    public void PerformSwim(StateInitializer init)
    {
        var inputReader = init.inputReader;
        float xMove = inputReader.movementInputVector.x;
        float zMove = inputReader.movementInputVector.y;

        Vector3 moveDir = Vector3.zero;

        if (xMove != 0 || zMove != 0)
        {


            if (inputReader.sprint && swimFastSpeed != currentSpeed)
            {
                animSystem.SwimNormal();
                currentSpeed = swimFastSpeed;
            }
            if (!inputReader.sprint && swimSpeed != currentSpeed)
            {
                animSystem.SwimNormal();
                currentSpeed = swimSpeed;
            }
            if (currentSpeed != lastCurrentSpeed)
            {

                lastCurrentSpeed = currentSpeed;
            }
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);

            Vector3 camForward = new Vector3(camHolder.forward.x, 0, camHolder.forward.z).normalized;
            Vector3 camRight = new Vector3(camHolder.right.x, 0, camHolder.right.z).normalized;

            moveDir = (camForward * zMove + camRight * xMove).normalized * currentSpeed;

            float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                angle + 90, transform.rotation.eulerAngles.z);

            rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
            //Debug.Log(targetRotation.eulerAngles);
            //transform.rotation = targetRotation;
        }
        else
        {
            //animator.SetFloat("move", 0);
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
            currentSpeed = 0;
            isStartSwim = true;
            if (currentSpeed != lastCurrentSpeed)
            {
                animSystem.Idle();
                lastCurrentSpeed = 0;
            }
        }
        float yMove = rb.velocity.y;

        rb.velocity = new Vector3(moveDir.x, 0, moveDir.z);
    }
    public void StartSwimming()
    {
        if (inputReader.sprint)
        {
            currentSpeed = swimFastSpeed;
            animSystem.SwimNormal();
        }
        else
        {
            currentSpeed = swimSpeed;
            animSystem.SwimNormal();
        }
        attackSystem.StopAnimationCountdown();
        attackSystem.ResetAttack();

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
    public void StopSwimming()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
}
