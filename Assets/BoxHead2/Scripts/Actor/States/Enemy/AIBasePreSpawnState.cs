using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBasePreSpawnState : AIBaseState
    {
        public AIBasePreSpawnState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            
            if (AIEnemy.AIConfig.preSpawnAnims != null && AIEnemy.AIConfig.preSpawnAnims.Length > 0)
            {
                AIEnemy.PlayAnimation(
                    AIEnemy.AIConfig.preSpawnAnims[Random.Range(0, AIEnemy.AIConfig.preSpawnAnims.Length)], 
                    0, 1, false, false);
            }
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (AIEnemy.AimedEnemy != null)
            {
                ChangeState<IIdleState>();
            }
        }
    }
}