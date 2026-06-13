using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace BoxHead2.States
{
    public class AIBaseRunAwayState : AIBaseState
    {
        Vector3 targetPos;
        
        public AIBaseRunAwayState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            targetPos = (SimpleMath.RandomOnCircleXZ() * 5f).normalized * 10 +  AIEnemy.Position;
            if (NavMesh.SamplePosition(targetPos, out var hit, 10, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }
            AIEnemy.MovePosition(targetPos, AIEnemy.RunSpeed, AIEnemy.RotateSpeed, 0.1f);
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    if (!AIEnemy.IsReachDestination())
                    {
                        AIEnemy.UpdateLocomotion(Vector2.up, Time.deltaTime);
                        AIEnemy.MovePosition(targetPos, AIEnemy.RunSpeed, AIEnemy.RotateSpeed, 0.1f);
                    }
                    else
                    {
                        ChangeState<IIdleState>();
                    }
                }
                else
                {
                    ChangeState<IIdleState>();
                }
            }
            else
            {
                ChangeState<IIdleState>();
            }
        }
    }
}