using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DL.StateMachine
{

    public abstract class StateBehaviour
    {
        public abstract void OnStateEnter(StateController stateController);
        public abstract void OnStateFixedUpdate(StateController stateController);
        public abstract void OnStateUpdate(StateController stateController);
        public abstract void OnStateExit(StateController stateController);

    }
}
