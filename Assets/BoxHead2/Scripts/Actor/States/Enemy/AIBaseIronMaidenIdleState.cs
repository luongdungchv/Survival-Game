using BoxHead2.Actor;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseIronMaidenIdleState : BaseAIBaseIdleState
    {
        public AIBaseIronMaidenIdleState(AIEnemy aiEnemy) : base(aiEnemy) 
        {
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
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