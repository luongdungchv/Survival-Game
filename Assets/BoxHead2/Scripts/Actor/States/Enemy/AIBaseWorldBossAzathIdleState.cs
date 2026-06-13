using BoxHead2.States;

namespace BoxHead2.Actor
{
    public class AIBaseWorldBossAzathIdleState : BaseAIBaseIdleState
    {
        public AIBaseWorldBossAzathIdleState(AIEnemy aiEnemy) : base(aiEnemy)
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