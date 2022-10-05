using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed, acceleratedSpeed, jumpSpeed, dashSpeed, dashDelay;

    public Transform camHolder, camHolderPos;
    public Vector2 mouseSensitivity;
    public AnimationClip swordClip;
    public ParticleSystem slashFX;
    public int attackMoves;
    public InputReader inputReader;


    public List<ParticleSystem> slashFXList;

    PlayerAttack attackSystem;
    PlayerAnimation animManager;


    public bool sprint, isOnGround, isStartMove;
    float currentSpeed, lastCurrentSpeed;


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
    }

    Coroutine rotationCoroutine;
    public void PerformMovement(StateInitializer init)
    {
        var inputReader = init.inputReader;
        float xMove = inputReader.movementInputVector.x;
        float zMove = inputReader.movementInputVector.y;

        Vector3 moveDir = Vector3.zero;

        if (xMove != 0 || zMove != 0)
        {
            if (inputReader.SprintPress())
            {
                if (canDash)
                {
                    animManager.Dash();
                    currentSpeed = dashSpeed;
                    StartCoroutine(DashCooldown());
                }
            }
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
            if (currentSpeed != lastCurrentSpeed)
            {
                animManager.Idle();
                lastCurrentSpeed = 0;
            }
        }
        float yMove = rb.velocity.y;

        rb.velocity = new Vector3(moveDir.x, yMove, moveDir.z);
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
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpSpeed, rb.velocity.z);
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



    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("hide");
    }
    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("show");
    }


}
