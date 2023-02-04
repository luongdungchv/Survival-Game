using System.Collections;
using UnityEngine;

[RequireComponent(typeof(InputReceiver), typeof(Rigidbody))]
public class NetworkMovement : MonoBehaviour
{
    // [SerializeField] private float speed, speedWhenConsuming, sprintSpeed, jumpSpeed, dashSpeed, maxFallingSpeed = 55;
    [SerializeField] private LayerMask slopeCheckMask;
    private NetworkPlayer netPlayer;
    private InputReceiver inputReceiver;
    private Rigidbody rb;
    private PlayerStats stats;
    private Vector2 movementInputVector => inputReceiver.movementInputVector;
    private bool groundCheck;
    private Coroutine rotationCoroutine;
    private Client client => Client.ins;
    private Vector3 moveDir;
    private float currentSpeed;
    private bool sprint => inputReceiver.sprint;
    private bool dash => inputReceiver.startDash;
    private bool jump => inputReceiver.jumpPress;
    private Vector2 camDir => inputReceiver.camDir;

    private float speed => stats.speed;
    private float sprintSpeed => stats.sprintSpeed;
    private float dashSpeed => stats.dashSpeed;
    private float speedWhenConsuming => speed / 2;
    private float jumpSpeed => stats.jumpSpeed;
    private float maxFallingSpeed => stats.maxFallingSpeed;

    private void Awake()
    {
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
        netPlayer = GetComponent<NetworkPlayer>();
        groundCheck = true;
    }
    private void Update()
    {
        if (rb.velocity.y < -maxFallingSpeed)
        {
            var vel = rb.velocity;
            vel.y = -maxFallingSpeed;
            rb.velocity = vel;
        }
    }
    public void MoveServer()
    {
        Debug.Log("moving");
        moveDir = new Vector3(camDir.x, 0, camDir.y) * movementInputVector.y
                        + new Vector3(camDir.y, 0, -camDir.x) * movementInputVector.x;
        currentSpeed = inputReceiver.isConsumingItem ? speed / 2 : (sprint ? sprintSpeed : speed);
        if (movementInputVector != Vector2.zero)
        {

            moveDir = moveDir.normalized;
            if (PerformSlopeCheck(ref moveDir))
                rb.velocity = moveDir * currentSpeed;

            float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
            rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
        }
    }
    private bool PerformSlopeCheck(ref Vector3 moveDir)
    {
        if (!groundCheck) return false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.26f, slopeCheckMask))
        {
            var slopeNormal = hit.normal;
            var tangent = Vector3.Cross(moveDir, hit.normal);
            var biTangent = Vector3.Cross(hit.normal, tangent);
            moveDir = biTangent.normalized;
            return true;
        }
        return false;
    }

    public void JumpServer()
    {
        if (!Client.ins.isHost) return;
        groundCheck = false;
        rb.velocity = Vector3.up * jumpSpeed + moveDir.normalized * currentSpeed;

    }
    public void DashServer()
    {
        moveDir = transform.forward;
        currentSpeed = dashSpeed;

        Debug.Log(netPlayer.id + " " + "dsash");
        moveDir = moveDir.normalized;
        if (PerformSlopeCheck(ref moveDir))
        {
            rb.velocity = moveDir * currentSpeed;
        }
    }
    public void StopMoveServer()
    {
        rb.velocity = Vector3.up * rb.velocity.y;
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
    private void OnCollisionEnter(Collision other)
    {
        groundCheck = true;
    }

}
