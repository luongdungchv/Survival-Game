using BoxHead2.States;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseQuillIdleState : BaseAIBaseIdleState
    {
        public AIBaseQuillIdleState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }
        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    if (AIEnemy.IsEnemyClose())
                    {
                        ChangeState<AIBaseRunAwayState>();
                    }
                    else
                    {
                        ChangeState<AIBasePatrolState>();
                    }
                }
                else
                {
                    if (AIEnemy.IsEnemyInAttackRange())
                    {
                        ChangeState<AIBaseAttackState>();
                    }
                    else
                    {
                        ChangeState<AIBaseChaseState>();
                    }
                }
            }
        }
    }
}