using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStateManager : MonoBehaviour
{
    public State Idle, Move, InAir, Attack, Sprint, SwimNormal, SwimIdle, SwimFast, Dash;
    private StateMachine fsm;
    private bool canDash = true;

    private NetworkMovement netMovement => GetComponent<NetworkMovement>();
    private NetworkSwimHandler netSwim => GetComponent<NetworkSwimHandler>();
    private InputReceiver inputReceiver => GetComponent<InputReceiver>();
    private PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    private void Awake()
    {


        fsm = GetComponent<StateMachine>();

        Idle.OnEnter.AddListener(() =>
        {
            netMovement.StopMoveServer();
            animSystem.CancelJump();
            animSystem.Idle();
        });
        InAir.OnEnter.AddListener(() =>
        {
            if (Client.ins.isHost) InAir.lockState = true;
            netMovement.JumpServer();
            animSystem.Jump();
        });
        Dash.OnEnter.AddListener(() => { animSystem.CancelJump(); animSystem.Dash(); });
        Move.OnEnter.AddListener(() => { animSystem.CancelJump(); animSystem.Walk(); });
        Sprint.OnEnter.AddListener(() =>
        {
            animSystem.CancelJump();
            animSystem.Run();
            Debug.Log("start sprint");
        });
        SwimIdle.OnEnter.AddListener(() =>
        {
            animSystem.CancelJump();
            netSwim.StopSwimServer();
            animSystem.SwimIdle();
        });
        SwimNormal.OnEnter.AddListener(() =>
        {
            animSystem.CancelJump();
            animSystem.SwimNormal();
        });

        if (!Client.ins.isHost) return;
        Dash.OnUpdate.AddListener(netMovement.DashServer);
        Move.OnUpdate.AddListener(netMovement.MoveServer);
        Sprint.OnUpdate.AddListener(() =>
        {
            netMovement.MoveServer();
            Debug.Log("sprinting");
        });
        SwimIdle.OnUpdate.AddListener(netSwim.SwimDetect);
        SwimNormal.OnUpdate.AddListener(netSwim.SwimServer);
        Idle.OnUpdate.AddListener(() => { });

    }
    private void Update()
    {
        if (!Client.ins.isHost) return;
        var inputVector = inputReceiver.movementInputVector;
        if (inputReceiver.startDash)
        {
            fsm.ChangeState(Dash);
        }
        else if (inputVector == Vector2.zero)
        {
            fsm.ChangeState(Idle);
        }
        else if (inputVector != Vector2.zero)
        {
            if (inputReceiver.sprint) fsm.ChangeState(Sprint);
            else fsm.ChangeState(Move);
        }
        if (inputReceiver.jumpPress)
        {
            fsm.ChangeState(InAir);
        }


    }
    private void OnCollisionEnter(Collision other)
    {
        if (fsm.currentState == InAir)
        {
            InAir.lockState = false;
            animSystem.CancelJump();
        }
    }
}
