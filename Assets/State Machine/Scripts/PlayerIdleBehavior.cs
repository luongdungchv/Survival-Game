using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DL.StateMachine;

public class PlayerIdleBehavior : StateBehaviour
{
    public override void OnStateEnter(StateController stateController)
    {
        Debug.Log("idle enter");
    }

    public override void OnStateExit(StateController stateController)
    {
        Debug.Log("idle exit");
    }

    public override void OnStateFixedUpdate(StateController stateController)
    {
        Debug.Log("idle fixed update");
    }

    public override void OnStateUpdate(StateController stateController)
    {
        Debug.Log("idle update");
    }
}
