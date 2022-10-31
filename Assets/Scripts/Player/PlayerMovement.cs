using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed, acceleratedSpeed, jumpSpeed, dashSpeed, dashDelay;

    public Transform camHolder, camHolderPos, slopeCheckPos;
    public Vector2 mouseSensitivity;
    public AnimationClip swordClip;
    public ParticleSystem slashFX;
    public int attackMoves;
    public InputReader inputReader;
    public LayerMask slopeCheckMask;


    public List<ParticleSystem> slashFXList;

    PlayerAttack attackSystem;
    PlayerAnimation animManager;


    public bool sprint, isOnGround, isStartMove;
    float currentSpeed, lastCurrentSpeed;
    private Vector3 moveDir;

    Rigidbody rb;
    public Animator animator;

    private bool canDash;
    private void Start()
    {
        //animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        attackSystem = GetComponent<PlayerAttack>();
        animManager = GetComponent<PlayerAnimation>();
        Debug.Log(Mathf.Atan2(-1, 0) * Mathf.Rad2Deg);
        camHolder.position = camHolderPos.position;
        currentSpeed = 0;
        lastCurrentSpeed = 0;
        isStartMove = true;
        isOnGround = true;
        canDash = true;

    }
    private void Update()
    {
        //PerformMovement();
        PerformRotation();
        camHolder.position = camHolderPos.position;

    }
    public void StopMoving()
    {
        rb.velocity = new Vector3(0, 0, 0);
        //        Debug.Log("stop");
    }

    Coroutine rotationCoroutine;
    public void PerformMovement(StateInitializer init)
    {
        var inputReader = init.inputReader;
        float xMove = inputReader.movementInputVector.x;
        float zMove = inputReader.movementInputVector.y;

        moveDir = new Vector3(0, rb.velocity.y, 0);

        if (xMove != 0 || zMove != 0)
        {
            // if (inputReader.SprintPress())
            // {
            //     if (canDash)
            //     {
            //         animManager.Dash();
            //         currentSpeed = dashSpeed;
            //         StartCoroutine(DashCooldown());
            //     }
            // }
            if (inputReader.sprint && acceleratedSpeed != currentSpeed && !animManager.animator.GetBool("dash"))
            {
                animManager.Run();
                currentSpeed = acceleratedSpeed;
            }
            if (!inputReader.sprint && moveSpeed != currentSpeed && !animManager.animator.GetBool("dash"))
            {
                animManager.Walk();
                currentSpeed = moveSpeed;
            }
            if (currentSpeed != lastCurrentSpeed)
            {
                lastCurrentSpeed = currentSpeed;
            }
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);

            Vector3 camForward = new Vector3(camHolder.forward.x, 0, camHolder.forward.z).normalized;
            Vector3 camRight = new Vector3(camHolder.right.x, 0, camHolder.right.z).normalized;

            moveDir = (camForward * zMove + camRight * xMove).normalized * currentSpeed;
            PerformSlopeCheck();


            float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
            rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
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
            isStartMove = true;
            moveDir = new Vector3(0, rb.velocity.y, 0);
            if (currentSpeed != lastCurrentSpeed)
            {
                animManager.Idle();
                lastCurrentSpeed = 0;
            }
        }
        //float yMove = rb.velocity.y;

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
        var jumpVelocity = transform.forward * currentSpeed + Vector3.up * jumpSpeed;
        // rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpSpeed, rb.velocity.z);
        rb.velocity = jumpVelocity;
        init.InAir.lockState = true;
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
        Debug.Log(inputReader.movementInputVector);
        if (inputReader.movementInputVector == Vector2.zero)
        {
            DisplaceForward(dashSpeed);
        }
        else
        {
            var inputDir = inputReader.movementInputVector;
            Vector3 camForward = new Vector3(camHolder.forward.x, 0, camHolder.forward.z).normalized;
            Vector3 camRight = new Vector3(camHolder.right.x, 0, camHolder.right.z).normalized;

            moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized * dashSpeed;


            float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
            transform.rotation = targetRotation;

            rb.velocity = moveDir;
        }
    }
    public void Dash(Vector3 dir)
    {
        rb.velocity = dir * dashSpeed;
    }
    private bool PerformSlopeCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(slopeCheckPos.transform.position, Vector3.down, out hit, 0.4f, slopeCheckMask))
        {
            var slopeNormal = hit.normal;
            var tangent = Vector3.Cross(moveDir, hit.normal);
            var biTangent = Vector3.Cross(hit.normal, tangent);
            moveDir = biTangent.normalized * currentSpeed;
            return true;
        }
        moveDir += Vector3.up * rb.velocity.y;
        return false;
    }

}
