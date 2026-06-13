using BoxHead2.States;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseSlimeIdleState : BaseAIBaseIdleState
    {
        public AIBaseSlimeIdleState(AIEnemy aiEnemy) : base(aiEnemy)
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
                        ChangeState<AIBaseStrafeState>();
                    }
                    else
                    {
                        ChangeState<AIBaseWalkNoSkillState>();
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
                        ChangeState<AIBaseWalkChaseState>();
                    }
                }
            }
            else if (Time.time - AIEnemy.LastTimePatrol > GameConstants.EnemyPatrolInterval)
            {
                ChangeState<AIBasePatrolState>();
            }
        }
    }
}