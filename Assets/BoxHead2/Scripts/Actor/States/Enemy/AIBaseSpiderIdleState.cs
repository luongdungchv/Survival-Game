using BoxHead2.States;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseSpiderIdleState : BaseAIBaseIdleState
    {
        public AIBaseSpiderIdleState(AIEnemy aiEnemy) : base(aiEnemy)
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
                        if (!AIEnemy.AimedEnemy.IsStaggering)
                        {
                            ChangeState<AIBaseRunAwayState>();
                        }
                    }
                    else
                    {
                        if (AIEnemy.AimedEnemy.IsStaggering)
                        {
                            ChangeState<AIBaseWalkNoSkillState>();   
                        }
                        else
                        {
                            ChangeState<AIBasePatrolState>();
                        }
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
            else if (Time.time - AIEnemy.LastTimePatrol > GameConstants.EnemyPatrolInterval)
            {
                ChangeState<AIBasePatrolState>();
            }
        }
    }
}