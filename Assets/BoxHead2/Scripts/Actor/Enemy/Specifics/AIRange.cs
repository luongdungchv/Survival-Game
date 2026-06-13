using BoxHead2.States;
using BoxHead2.Utils;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIRange : AIEnemy
    {
        [SerializeField] public bool stayStill;
        protected override void InitStateMachine()
        {
            StateMachine = new Dacodelaac.FiniteStateMachine.StateMachine();
            StateMachine.InitStates(
                new AppearBaseState(this),
                new AIBaseRangeEnemyIdleState(this),
                new AIBaseChaseState(this),
                new AIBasePatrolState(this),
                new AIBaseAttackState(this),
                new AIBaseRunAwayState(this),
                new KnockdownBaseState(this),
                new GetHitBaseState(this),
                new FreezeBaseState(this),
                new StaggerBaseState(this),
                new DeadBaseExplodeState(this));
            StateMachine.ChangeState<IIdleState>();
        }
        
        public override float GetCustomSkillCondition(int index)
        {
            if (index == 1)
            {
                return RemoteConfigUtils.ABTestEnemyValue;
            }
            return base.GetCustomSkillCondition(index);
        }
    }
}