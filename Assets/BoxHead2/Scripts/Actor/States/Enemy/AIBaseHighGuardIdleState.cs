using BoxHead2.States;

namespace BoxHead2.Actor
{
    public class AIBaseHighGuardIdleState : BaseAIBaseIdleState
    {
        public AIBaseHighGuardIdleState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }

        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    ChangeState<AIBasePatrolState>();
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