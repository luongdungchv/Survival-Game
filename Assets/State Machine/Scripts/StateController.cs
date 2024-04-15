using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DL.StateMachine
{
    public class StateController : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private StateMachine fsm;
        [SerializeField] private StateMachineDataSO stateData;

        private void Awake()
        {
#if UNITY_EDITOR
            stateData.stateList.ForEach(x => x.GenerateType());
#endif
            fsm = new StateMachine(this);
            fsm.BuildStateMachineFromData(stateData);
        }

        private void Update()
        {
            this.fsm.OnStateUpdate();
        }
        private void FixedUpdate()
        {
            this.fsm.OnStateFixedUpdate();
        }
        [Sirenix.OdinInspector.Button]
        public void ChangeState(string state)
        {
            this.fsm.ChangeState(state);
        }

    }

    [System.Serializable]
    public class StateMachine
    {
        [SerializeField] private Dictionary<string, State> stateMap;

        private StateController controller;
        [SerializeField] private State currentState;
        public State CurrentState => this.currentState;

        public StateMachine(StateController controller)
        {
            this.controller = controller;
        }

        public void ChangeState(string stateName)
        {
            if (!stateMap.ContainsKey(stateName)) return;
            if (currentState == null)
            {
                currentState = stateMap[stateName];
                currentState.OnStateEnter(controller);
                return;
            }
            if (currentState.CanTransitionTo(stateName))
            {
                currentState?.OnStateExit(controller);
                currentState = stateMap[stateName];
                currentState?.OnStateEnter(controller);
            }
        }

        public void BuildStateMachineFromData(StateMachineDataSO data)
        {
            this.stateMap ??= new Dictionary<string, State>();
            this.stateMap.Clear();
            foreach (var holder in data.stateList)
            {
                if (stateMap.ContainsKey(holder.name)) continue;
                var state = new State(holder.name);

                var behaviourList = holder.CreateBehaviourInstance();
                UnityAction<StateController> onEnterCallback = (c) => { };
                UnityAction<StateController> onExitCallback = (c) => { };
                UnityAction<StateController> onUpdateCallback = (c) => { };
                UnityAction<StateController> onFixedUpdateCallback = (c) => { };
                foreach (var behaviour in behaviourList)
                {
                    var stateBehaviour = (StateBehaviour)behaviour;
                    onEnterCallback += stateBehaviour.OnStateEnter;
                    onExitCallback += stateBehaviour.OnStateExit;
                    onUpdateCallback += stateBehaviour.OnStateUpdate;
                    onFixedUpdateCallback += stateBehaviour.OnStateFixedUpdate;
                }

                state.SetCallback(onEnterCallback, onExitCallback, onUpdateCallback, onFixedUpdateCallback);
                stateMap.Add(state.Name, state);
            }
            foreach (var transition in data.stateTransitionList)
            {
                var startHolder = data.stateList[transition.startIndex];
                var endHolder = data.stateList[transition.endIndex];
                stateMap[startHolder.name].AddStateToTransition(stateMap[endHolder.name]);
            }

            ChangeState(data.stateList[data.entryStateIndex].name);
        }

        public void OnStateEnter()
        {
            this.CurrentState.OnStateEnter(controller);
        }

        public void OnStateUpdate()
        {
            this.CurrentState.OnStateUpdate(controller);
        }

        public void OnStateExit()
        {
            this.CurrentState.OnStateExit(controller);
        }

        public void OnStateFixedUpdate()
        {
            this.CurrentState.OnStateFixedUpdate(controller);
        }
    }
    [System.Serializable]
    public class State
    {
        [SerializeField] private string name;
        public string Name => this.name;

        private UnityAction<StateController> OnEnter, OnExit, OnUpdate, OnFixedUpdate;

        private List<State> transitionList;

        public State(string name)
        {
            this.name = name;
            this.transitionList = new List<State>();
        }

        public void AddStateToTransition(State state)
        {
            this.transitionList.Add(state);
        }

        public bool CanTransitionTo(string stateName)
        {
            foreach (var state in this.transitionList)
            {
                if (state.name == stateName) return true;
            }
            return false;
        }

        public void SetCallback(
            UnityAction<StateController> OnEnter,
            UnityAction<StateController> OnExit,
            UnityAction<StateController> OnUpdate,
            UnityAction<StateController> OnFixedUpdate
        )
        {
            this.OnEnter = OnEnter;
            this.OnExit = OnExit;
            this.OnUpdate = OnUpdate;
            this.OnFixedUpdate = OnFixedUpdate;
        }

        public void OnStateEnter(StateController controller)
        {
            this.OnEnter.Invoke(controller);
        }

        public void OnStateUpdate(StateController controller)
        {
            this.OnUpdate.Invoke(controller);
        }

        public void OnStateExit(StateController controller)
        {
            this.OnExit.Invoke(controller);
        }

        public void OnStateFixedUpdate(StateController controller)
        {
            this.OnFixedUpdate.Invoke(controller);
        }
    }
}