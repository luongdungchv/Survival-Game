using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dacodelaac.FiniteStateMachine
{
    public class StateMachine
    {
        public BaseState CurrentState { get; private set; }

        public BaseState[] States { get; private set; }

        public void InitStates(params BaseState[] states)
        {
            States = states;
            foreach (var state in States)
            {
                state.StateMachine = this;
            }
        }

        public void ChangeState<T>(object data = null) where T : IState
        {
            var state = States.FirstOrDefault(s => s is T);
            if (state == null)
            {
                Debug.LogError($"State {typeof(T)} not found");
            }
            ChangeState(state, data);
        }

        void ChangeState(BaseState baseState, object data = null)
        {
            if (baseState == this.CurrentState) return;
            // Dacoder.LogError(CurrentState + " " + state);
            var oldState = this.CurrentState;
            this.CurrentState = baseState;
            oldState?.StateExit(baseState);
            baseState?.StateEnter(oldState, data);
        }

        public void Tick()
        {
            this.CurrentState?.StateTick();
        }

        public void FixedTick()
        {
            this.CurrentState?.StateFixedTick();
        }

        public void RemoveAllStatesExcept<T>() where T : IState
        {
            States = States.Where(x => x is T).ToArray();
        }
    }
}