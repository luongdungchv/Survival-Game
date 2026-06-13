using BoxHead2.States;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseWitch2IdleState : BaseAIBaseIdleState
    {
        public AIBaseWitch2IdleState(AIEnemy aiEnemy) : base(aiEnemy)
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
                    else if (AIEnemy.IsEnemyClose())
                    {
                        ChangeState<AIBaseRunAwayUtilSkillState>();
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