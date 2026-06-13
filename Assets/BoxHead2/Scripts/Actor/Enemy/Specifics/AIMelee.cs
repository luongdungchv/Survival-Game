using BoxHead2.Actor;
using BoxHead2.Skills;
using BoxHead2.States;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIMelee : AIEnemy
    {
        [SerializeField] bool useRunaway, useFastChase;
        [SerializeField] bool stayStill;
        [SerializeField] bool skipAppear;
        protected override void InitStateMachine()
        {
            StateMachine = new Dacodelaac.FiniteStateMachine.StateMachine();
            StateMachine.InitStates(
                new AIBaseMeleeIdleState(this, useRunaway, useFastChase, stayStill),
                new AIBasePatrolState(this),
                new AIBaseWalkNoSkillState(this),
                new AIBaseWalkChaseState(this),
                new AIBaseChaseState(this),
                new AIBaseStrafeState(this),
                new AIBaseAttackState(this),
                new DeadBaseExplodeState(this),
                new AIBaseRunAwayState(this),
                new GetHitBaseState(this),
                new StaggerBaseState(this),
                new AppearBaseState(this, skipAppear));
            StateMachine.ChangeState<IIdleState>();
        }
        
    }
}