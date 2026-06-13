using BoxHead2.Actor;
using BoxHead2.Utils;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseMeleeIdleState : BaseAIBaseIdleState
    {
        readonly bool _runaway;
        readonly bool _fastChase;
        readonly bool _stayStill;

        public AIBaseMeleeIdleState(AIEnemy aiEnemy, bool runaway, bool useFastChase, bool stayStill) : base(aiEnemy)
        {
            _runaway = runaway;
            _fastChase = useFastChase;
            _stayStill = stayStill;
            AIEnemy.UpdateLocomotion(Vector2.zero);
        }

        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (_stayStill) return;
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    if (_runaway) ChangeState<AIBaseRunAwayState>();
                    else ChangeState<AIBaseStrafeState>();
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