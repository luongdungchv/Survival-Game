using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateInitializer : MonoBehaviour
{
    public State Idle, Move, InAir, Attack, Sprint, SwimNormal, SwimIdle, SwimFast;
    public InputReader inputReader;
    public StateMachine fsm;


    PlayerMovement movementSystem => GetComponent<PlayerMovement>();
    PlayerAttack attackSystem => GetComponent<PlayerAttack>();
    PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    SwimHandler swimSystem => GetComponent<SwimHandler>();

    private void Awake()
    {
        Idle.OnUpdate.AddListener(() => { });
        Idle.OnEnter.AddListener(() =>
        {
            Debug.Log("Idle");
            animSystem.Idle();
            movementSystem.StopMoving();
        });

        Move.OnEnter.AddListener(movementSystem.StartMove);
        Move.OnUpdate.AddListener(() => movementSystem.PerformMovement(this));

        InAir.OnEnter.AddListener(() => movementSystem.PerformJump(this));

        Attack.OnUpdate.AddListener(() => attackSystem.PerformAttack(inputReader, this));
        Attack.OnEnter.AddListener(movementSystem.StopMoving);

        SwimIdle.OnEnter.AddListener(() =>
        {
            animSystem.SwimIdle();
            swimSystem.StopSwimming();
            Debug.Log("Swim Idle");
        });

        SwimNormal.OnEnter.AddListener(() => swimSystem.StartSwimming());
        SwimNormal.OnUpdate.AddListener(() => swimSystem.PerformSwim(this));
    }
    private void Update()
    {
        if (inputReader.SlashPress())
        {
            fsm.ChangeState(Attack);
        }
        else if (inputReader.JumpPress())
        {

            fsm.ChangeState(InAir);


        }

        else if (inputReader.movementInputVector != Vector2.zero)
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
        else
        {
            var curStateName = fsm.currentState.name;
            if (!curStateName.Contains("Swim") || animSystem.animator.GetFloat("swim") < 0.03f)
            {
                //Debug.Log(curStateName);
                fsm.ChangeState(Idle);
            }
            else
            {
                fsm.ChangeState(SwimIdle);
            }
        }
    }
    private async void OnCollisionEnter(Collision collision)
    {

        if (fsm.currentState.name == "InAir")
        {
            Debug.Log(collision.gameObject);
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
