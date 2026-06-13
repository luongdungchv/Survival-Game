using System;
using UnityEngine;

namespace Dacodelaac.FiniteStateMachine
{
    public class BaseState : IState
    {
        public StateMachine StateMachine { get; set; }

        public void ChangeState<T>(object data = null) where T : IState
        {
            if (StateMachine.CurrentState != this)
            {
                Debug.LogError($"Changing from invalid state {this}/{StateMachine.CurrentState}->{typeof(T)}");
                return;
            }
            StateMachine.ChangeState<T>(data);
        }
        
        public void StateEnter(BaseState from, object data)
        {
            OnStateEnter(from, data);
        }

        public void StateTick()
        {
            OnStateUpdate();
        }

        public void StateFixedTick()
        {
            OnStateFixedUpdate();
        }

        public void StateExit(BaseState to)
        {
            OnStateExit(to);
        }

        protected virtual void OnStateEnter(BaseState from, object data)
        {
        }

        protected virtual void OnStateUpdate()
        {
        }

        protected virtual void OnStateFixedUpdate()
        {
        }

        protected virtual void OnStateExit(BaseState to)
        {
        }

        public virtual void OnCustomEvent(int index)
        {
        }
    }
}