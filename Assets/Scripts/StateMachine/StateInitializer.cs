using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateInitializer : MonoBehaviour
{
    public State Idle, Move, InAir, Attack, Sprint, SwimNormal, SwimIdle, SwimFast, Dash;
    public InputReader inputReader;
    public StateMachine fsm;
    public static StateInitializer ins;
    private bool canDash = true;
    [SerializeField] private bool isGround = true;
    [SerializeField] private LayerMask groundMask;


    PlayerMovement movementSystem => GetComponent<PlayerMovement>();
    PlayerAttack attackSystem => GetComponent<PlayerAttack>();
    PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    SwimHandler swimSystem => GetComponent<SwimHandler>();
    PlayerStats stats => GetComponent<PlayerStats>();

    private void Awake()
    {
        if (ins == null) ins = this;
        Idle.OnUpdate.AddListener(() => { });
        Idle.OnEnter.AddListener(() =>
        {
            animSystem.Idle();
            movementSystem.StopMoving();
        });

        Move.OnEnter.AddListener(movementSystem.StartMove);
        Move.OnUpdate.AddListener(() => movementSystem.PerformMovement(this));

        InAir.OnEnter.AddListener(() => movementSystem.PerformJump(this));

        Attack.OnUpdate.AddListener(() => attackSystem.PerformAttack());
        Attack.OnEnter.AddListener(movementSystem.StopMoving);

        SwimIdle.OnEnter.AddListener(() =>
        {
            animSystem.SwimIdle();
            swimSystem.StopSwimming();
        });

        SwimNormal.OnEnter.AddListener(() => swimSystem.StartSwimming());
        SwimNormal.OnUpdate.AddListener(() => swimSystem.PerformSwim(this));

        Dash.OnEnter.AddListener(() =>
        {
            if (canDash)
            {
                StartCoroutine(DashCooldown());
                animSystem.Dash();
                stats.DashDecrease();
            }
        });

        Dash.OnUpdate.AddListener(() =>
        {
            movementSystem.Dash();

        });
    }
    private void Update()
    {



        if (inputReader.SprintPress() && stats.stamina > 0 && !PlayerEquipment.ins.isConsumingItem)
        {
            fsm.ChangeState(Dash, true);
        }
        else if (inputReader.SlashPress())
        {
            PlayerEquipment.ins.OnUsePress();
        }
        else if (inputReader.SlashRelease())
        {
            PlayerEquipment.ins.OnUseReleases();
        }
        else if (inputReader.JumpPress() && !PlayerEquipment.ins.isConsumingItem)
        {
            fsm.ChangeState(InAir);
        }

        else if (inputReader.movementInputVector != Vector2.zero && !animSystem.animator.GetBool("dash"))
        {
            var curStateName = fsm.currentState.name;
            if (!curStateName.Contains("Swim"))
            {
                fsm.ChangeState(Move);
            }
            else
            {
                fsm.ChangeState(SwimNormal);
            }
        }
        else if (!animSystem.animator.GetBool("dash"))
        {
            var curStateName = fsm.currentState.name;
            //bool dash = animSystem.animator.GetBool("dash");
            if (!curStateName.Contains("Swim") || animSystem.animator.GetFloat("swim") < 0.001f)
            {
                fsm.ChangeState(Idle);
            }
            else
            {
                fsm.ChangeState(SwimIdle);
            }
        }
    }
    IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(movementSystem.dashDelay);
        canDash = true;
    }
    private async void OnCollisionEnter(Collision collision)
    {

        if (fsm.currentState.name == "InAir")
        {
            InAir.lockState = false;
            animSystem.CancelJump();
            var moveParam = animSystem.animator.GetFloat("move");
            if (moveParam < 0.1f)
            {
                await fsm.ChangeState(Idle, 100);
            }
            else if (moveParam > 0.4f)
            {
                await fsm.ChangeState(Move, 100);
            }
        }
    }

}
