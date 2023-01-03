using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed, acceleratedSpeed, jumpSpeed, dashSpeed, dashJumpSpeed, dashDelay;

    public Transform camHolder, camHolderPos, slopeCheckPos;
    public Vector2 mouseSensitivity;
    public AnimationClip swordClip;
    public ParticleSystem slashFX;
    public int attackMoves, testForce;
    public InputReader inputReader;
    public LayerMask slopeCheckMask;


    public List<ParticleSystem> slashFXList;

    PlayerAttack attackSystem;
    PlayerAnimation animManager;
    PlayerStats stats;
    public bool sprint, isOnGround, isStartMove;
    public float currentSpeed, lastCurrentSpeed;
    private Vector3 moveDir;

    private Rigidbody rb;

    private bool canDash;
    private void Start()
    {
        //animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        attackSystem = GetComponent<PlayerAttack>();
        animManager = GetComponent<PlayerAnimation>();
        stats = GetComponent<PlayerStats>();

        camHolder.position = camHolderPos.position;

        currentSpeed = 0;
        lastCurrentSpeed = 0;
        isStartMove = true;
        isOnGround = true;
        canDash = true;

    }
    private void Update()
    {
        PerformRotation();
        camHolder.position = camHolderPos.position;
    }
    public void SyncCamera()
    {
        camHolder.position = camHolderPos.position;
    }
    public void StopMoving()
    {
        rb.velocity = new Vector3(0, 0, 0);
        currentSpeed = 0;
    }

    Coroutine rotationCoroutine;
    public void PerformMovement(StateInitializer init)
    {

        float xMove = inputReader.movementInputVector.x;
        float zMove = inputReader.movementInputVector.y;

        moveDir = new Vector3(0, rb.velocity.y, 0);
        bool isConsumingItem = PlayerEquipment.ins.isConsumingItem;

        if (xMove != 0 || zMove != 0)
        {

            if (inputReader.sprint &&
                acceleratedSpeed != currentSpeed &&
                !animManager.animator.GetBool("dash") &&
                stats.stamina > 0 &&
                !isConsumingItem)
            {
                animManager.Run();
                currentSpeed = acceleratedSpeed;
            }
            if ((!inputReader.sprint || stats.stamina <= 0) && moveSpeed != currentSpeed && !animManager.animator.GetBool("dash"))
            {
                animManager.Walk();
                currentSpeed = isConsumingItem ? moveSpeed / 2 : moveSpeed;
            }
            if (currentSpeed != lastCurrentSpeed)
            {
                lastCurrentSpeed = currentSpeed;
            }
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);

            if (currentSpeed == acceleratedSpeed) stats.SprintDecrease();

            Vector3 camForward = new Vector3(camHolder.forward.x, 0, camHolder.forward.z).normalized;
            Vector3 camRight = new Vector3(camHolder.right.x, 0, camHolder.right.z).normalized;

            moveDir = (camForward * zMove + camRight * xMove).normalized;
            PerformSlopeCheck();


            if (Client.ins.isHost)
            {
                float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
                rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
            }
        }
        else
        {
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
            currentSpeed = 0;
            isStartMove = true;
            moveDir = new Vector3(0, rb.velocity.y, 0);
            if (currentSpeed != lastCurrentSpeed)
            {
                animManager.Idle();
                lastCurrentSpeed = 0;
            }
        }
        if (Client.ins.isHost)
            rb.velocity = new Vector3(moveDir.x, moveDir.y, moveDir.z);
    }
    public void StartMove()
    {
        if (inputReader.sprint)
        {
            currentSpeed = acceleratedSpeed;
            animManager.Run();
        }
        else
        {
            currentSpeed = moveSpeed;
            animManager.Walk();
        }
        attackSystem.StopAnimationCountdown();
        attackSystem.ResetAttack();

    }
    public void PerformJump(StateInitializer init)
    {
        animManager.Jump();
        init.InAir.lockState = true;
        if (Client.ins.isHost)
        {
            float horizontalJump = currentSpeed == dashSpeed ? dashJumpSpeed : currentSpeed;
            var jumpVelocity = transform.forward * horizontalJump + Vector3.up * jumpSpeed;
            rb.velocity = jumpVelocity;

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
    IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashDelay);
        canDash = true;
    }

    float xRotation, yRotation;
    void PerformRotation()
    {
        xRotation += -inputReader.MouseY() * mouseSensitivity.x;
        xRotation = Mathf.Clamp(xRotation, -85, 85);
        yRotation += inputReader.MouseX() * mouseSensitivity.y;
        camHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, camHolder.transform.rotation.z);
    }
    public void DisplaceForward(float magnitude)
    {
        rb.velocity = transform.forward * magnitude;
    }
    public void Dash()
    {
        if (!Client.ins.isHost) return;
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        currentSpeed = dashSpeed;
        if (inputReader.movementInputVector == Vector2.zero)
        {
            moveDir = transform.forward;
            PerformSlopeCheck();
            rb.velocity = moveDir.normalized * dashSpeed;
        }
        else
        {
            var inputDir = inputReader.movementInputVector;
            Vector3 camForward = new Vector3(camHolder.forward.x, 0, camHolder.forward.z).normalized;
            Vector3 camRight = new Vector3(camHolder.right.x, 0, camHolder.right.z).normalized;

            moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;
            PerformSlopeCheck();
            moveDir = moveDir.normalized * dashSpeed;

            var flatMoveDir = moveDir;
            flatMoveDir.y = 0;
            flatMoveDir = flatMoveDir.normalized;

            var projectedMovedir = (new Vector3(moveDir.x, 0, moveDir.z)).normalized;
            float angle = -Mathf.Atan2(projectedMovedir.z, projectedMovedir.x) * Mathf.Rad2Deg;
            Vector3 rotationVector = new Vector3(transform.rotation.x, angle + 90, transform.rotation.z);
            Quaternion targetRotation = Quaternion.Euler(rotationVector);
            transform.rotation = targetRotation;

            rb.AddForce(0, testForce, 0);
            rb.velocity = moveDir;
        }
    }
    public void FixedVerticalVel()
    {
        RaycastHit hit;
        if (Physics.Raycast(slopeCheckPos.transform.position, Vector3.down, out hit, 0.4f, slopeCheckMask))
        {
            var vel = rb.velocity;
            if (vel.y < 0) vel.y = 0;
            rb.velocity = vel;
        }
    }

    private bool PerformSlopeCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(slopeCheckPos.transform.position, Vector3.down, out hit, 0.4f, slopeCheckMask))
        {
            var slopeNormal = hit.normal;
            var tangent = Vector3.Cross(moveDir, hit.normal);
            var biTangent = Vector3.Cross(hit.normal, tangent);
            float restraint = PlayerEquipment.ins.isConsumingItem ? 1f / 2f : 1;
            moveDir = biTangent.normalized * currentSpeed * restraint;

            return true;
        }
        moveDir += Vector3.up * rb.velocity.y;
        return false;
    }
    public void ResetStats()
    {
        this.moveSpeed = 0;
        this.dashSpeed = 0;
        this.acceleratedSpeed = 0;
        this.jumpSpeed = 0;
    }

}
