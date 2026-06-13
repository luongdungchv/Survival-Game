using Dacodelaac.FiniteStateMachine;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Actor
{
    public class AIBaseRunAwayUtilSkillState : AIBaseState
    {
        Vector3 targetPos;
        
        public AIBaseRunAwayUtilSkillState(AIEnemy actor) : base(actor)
        {
        }
        
        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            targetPos = SimpleMath.RandomOnCircleXZ().normalized * 10 + AIEnemy.Position;
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
                if (!AIEnemy.IsEnemyInAttackRange() && AIEnemy.IsEnemyClose())
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