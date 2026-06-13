using BoxHead2.States;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseRangeEnemyIdleState : BaseAIBaseIdleState
    {
        AIRange _aiRange;
        public AIBaseRangeEnemyIdleState(AIEnemy aiEnemy) : base(aiEnemy)
        {
            _aiRange = aiEnemy as AIRange;
        }
        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (_aiRange is { stayStill: true })
            {
                if (AIEnemy.AimedEnemy != null && AIEnemy.IsEnemyInAttackRange() && AIEnemy.IsAttackTimeMatch)
                {
                    ChangeState<AIBaseAttackState>();
                }

                return;
            }
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
            else if (Time.time - AIEnemy.LastTimePatrol > GameConstants.EnemyPatrolInterval)
            {
                if (_aiRange is { stayStill: false }) ChangeState<AIBasePatrolState>();
            }
        }
    }
}