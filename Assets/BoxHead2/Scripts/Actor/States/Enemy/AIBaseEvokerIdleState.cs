using BoxHead2.States;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseEvokerIdleState : BaseAIBaseIdleState
    {
        public AIBaseEvokerIdleState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }
        
        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    ChangeState<AIBaseRunAwayState>();
                }
                else
                {
                    if (AIEnemy.IsEnemyInAttackRange())
                    {
                        ChangeState<AIBaseAttackState>();
                    }
                    else ChangeState<AIBaseChaseState>();
                }
            }
            else
            {
                ChangeState<AIBasePatrolState>();
            }
        }
    }
}