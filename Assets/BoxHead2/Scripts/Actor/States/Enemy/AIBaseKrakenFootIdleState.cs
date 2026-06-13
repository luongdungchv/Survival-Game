using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseKrakenFootIdleState : BaseAIBaseIdleState
    {
        public AIBaseKrakenFootIdleState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }
        
        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.PlayAnimation("locomotion", 0, 1, true, true);
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (AIEnemy.AimedEnemy != null)
            {
                if (AIEnemy.IsAttackTimeMatch && AIEnemy.IsEnemyInAttackRange())
                {
                    ChangeState<AIBaseAttackState>();
                }
                else
                {
                    AIEnemy.RotateDirection(AIEnemy.AimedDirection, AIEnemy.RotateSpeed);
                }
            }
        }
    }
}