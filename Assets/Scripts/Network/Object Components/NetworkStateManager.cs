using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkStateManager : MonoBehaviour
{
    public State Idle, Move, InAir, Attack, Sprint, SwimNormal, SwimIdle, SwimFast, Dash;
    private StateMachine fsm;
    private Dictionary<string, State> stateDictionary;
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
        PopulateStatsDictionary();
        InitializeStates3();
        fsm.currentState = Idle;

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
    private void PopulateStatsDictionary()
    {
        stateDictionary = new Dictionary<string, State>();
        stateDictionary.Add(Idle.name, Idle);
        stateDictionary.Add(Move.name, Move);

        stateDictionary.Add(InAir.name, InAir);

        stateDictionary.Add(Attack.name, Attack);

        stateDictionary.Add(Sprint.name, Sprint);

        stateDictionary.Add(Dash.name, Dash);

        stateDictionary.Add(SwimNormal.name, SwimNormal);

        stateDictionary.Add(SwimIdle.name, SwimIdle);

        //        stateDictionary.Add(SwimFast.name, SwimFast);

    }
    private void InitializeStates()
    {
        Idle = ScriptableObject.CreateInstance("State") as State;
        Idle.name = "Idle";
        Move = ScriptableObject.CreateInstance("State") as State;
        Move.name = "Move";
        InAir = ScriptableObject.CreateInstance("State") as State;
        InAir.name = "InAir";
        Attack = ScriptableObject.CreateInstance("State") as State;
        Attack.name = "Attack";
        Sprint = ScriptableObject.CreateInstance("State") as State;
        Sprint.name = "Sprint";
        Dash = ScriptableObject.CreateInstance("State") as State;
        Dash.name = "Dash";
        SwimNormal = ScriptableObject.CreateInstance("State") as State;
        SwimNormal.name = "SwimNormal";
        SwimIdle = ScriptableObject.CreateInstance("State") as State;
        SwimIdle.name = "SwimIdle";

        //Idle.transitions.AddRange()
        // SwimFast = stateDictionary["SwimFast"];
    }
    private void InitializeStates2()
    {
        HashSet<string> clonedNames = new HashSet<string>();
        var keys = stateDictionary.Keys.ToList();
        foreach (var i in keys)
        {

            if (stateDictionary[i] == null) continue;
            var clonedState = ScriptableObject.Instantiate(stateDictionary[i]);
            clonedState.name = clonedState.name.Remove(clonedState.name.Length - 7);
            if (clonedNames.Contains(clonedState.name)) continue;
            clonedNames.Add(clonedState.name);
            stateDictionary[clonedState.name] = clonedState;
            for (int j = 0; j < clonedState.transitions.Count; j++)
            {
                var baseTransition = clonedState.transitions[j];
                if (clonedNames.Contains(baseTransition.name))
                {
                    clonedState.transitions[j] = stateDictionary[baseTransition.name];
                    continue;
                }
                var clonedTransition = ScriptableObject.Instantiate(baseTransition);
                clonedTransition.name = clonedTransition.name.Remove(clonedTransition.name.Length - 7);
                clonedState.transitions[j] = clonedTransition;
                stateDictionary[clonedTransition.name] = clonedTransition;
                clonedNames.Add(clonedTransition.name);
            }
        }
        foreach (var i in stateDictionary.Keys)
        {
            var state = stateDictionary[i];
            foreach (var j in state.transitions)
            {
                Debug.Log(j.name + " " + state.name);
            }
        }
        Debug.Log(Idle == stateDictionary["Idle"]);
        Idle = stateDictionary["Idle"];
        Move = stateDictionary["Move"];
        InAir = stateDictionary["InAir"];
        Attack = stateDictionary["Attack"];
        Sprint = stateDictionary["Sprint"];
        Dash = stateDictionary["Dash"];
        SwimNormal = stateDictionary["SwimNormal"];
        SwimIdle = stateDictionary["SwimIdle"];
    }
    private void InitializeStates3()
    {
        var keys = stateDictionary.Keys.ToList();
        foreach (var i in keys)
        {
            stateDictionary[i] = ScriptableObject.Instantiate(stateDictionary[i]);
            stateDictionary[i].name = stateDictionary[i].name.Remove(stateDictionary[i].name.Length - 7);
        }
        State[] states = { Idle, Move, InAir, Attack, Sprint, Dash, SwimIdle, SwimNormal };
        foreach (var i in keys)
        {
            var state = stateDictionary[i];
            for (int j = 0; j < state.transitions.Count; j++)
            {
                state.transitions[j] = stateDictionary[state.transitions[j].name];
            }
        }
        Idle = stateDictionary["Idle"];
        Move = stateDictionary["Move"];
        InAir = stateDictionary["InAir"];
        Attack = stateDictionary["Attack"];
        Sprint = stateDictionary["Sprint"];
        Dash = stateDictionary["Dash"];
        SwimNormal = stateDictionary["SwimNormal"];
        SwimIdle = stateDictionary["SwimIdle"];
    }
}
