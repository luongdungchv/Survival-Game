﻿using System.Collections;
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
    private NetworkEquipment equipmentSystem => GetComponent<NetworkEquipment>();
    private NetworkAttack attackSystem => GetComponent<NetworkAttack>();
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

        Attack.OnEnter.AddListener(() =>
        {
            netMovement.StopMoveServer();
        });
        Attack.OnUpdate.AddListener(() =>
        {
            attackSystem.AttackServer();
        });
        Attack.OnExit.AddListener(attackSystem.ResetAttack);



        if (!Client.ins.isHost) return;
        Dash.OnFixedUpdate.AddListener(netMovement.DashServer);
        Move.OnFixedUpdate.AddListener(netMovement.MoveServer);
        Sprint.OnFixedUpdate.AddListener(() =>
        {
            netMovement.MoveServer();
        });
        SwimIdle.OnFixedUpdate.AddListener(netSwim.SwimDetect);
        SwimNormal.OnFixedUpdate.AddListener(netSwim.SwimServer);
        //Idle.OnFixedUpdate.AddListener(() => { });

    }
    private void FixedUpdate()
    {
        if (!Client.ins.isHost) return;
        var inputVector = inputReceiver.movementInputVector;

        if (inputReceiver.startDash)
        {
            fsm.ChangeState(Dash);
        }
        else if (inputReceiver.attack)
        {
            equipmentSystem.Use();
        }
        else if (inputVector != Vector2.zero)
        {
            if (inputReceiver.sprint && !inputReceiver.isConsumingItem) fsm.ChangeState(Sprint);
            else fsm.ChangeState(Move);
        }

        else if (inputVector == Vector2.zero)
        {
            fsm.ChangeState(Idle);
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
