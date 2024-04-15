using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DL.StateMachine;

public class PlayerRunBehaviour : StateBehaviour
{
    public override void OnStateEnter(StateController stateController)
    {
        Debug.Log("state endter");
    }   

    public override void OnStateExit(StateController stateController)
    {
        Debug.Log("run exit");
    }

    public override void OnStateFixedUpdate(StateController stateController)
    {
        Debug.Log("run fixed update");
    }

    public override void OnStateUpdate(StateController stateController)
    {
        Debug.Log("run update");
    }
}
