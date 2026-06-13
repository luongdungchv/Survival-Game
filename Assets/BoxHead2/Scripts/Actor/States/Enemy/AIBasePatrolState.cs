using System.Collections;
using System.Collections.Generic;
using Dacodelaac.FiniteStateMachine;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Actor
{
    public class AIBasePatrolState : AIBaseState
    {
        float Speed => runWhilePatrol ? AIEnemy.RunSpeed : AIEnemy.WalkSpeed;
        float AnimLocomotionRate => runWhilePatrol ? 1f : 0.5f;
        
        Vector3 targetPosition;
        float patrolTime;
        bool stopIfEnemyClose;
        bool runWhilePatrol;
        float patrolMaxDistance;
        Vector3 lastPos;
        float updateLastPosTime;
        bool rootMotion;
        float rootMotionMult;
        
        public AIBasePatrolState(AIEnemy aiEnemy, bool stopIfEnemyClose = true, bool runWhilePatrol = false, 
            float patrolMaxDistance = 2.5f, bool rootMotion = false, float rootMotionMult = 1) : base(aiEnemy)
        {
            this.stopIfEnemyClose = stopIfEnemyClose;
            this.runWhilePatrol = runWhilePatrol;
            this.patrolMaxDistance = patrolMaxDistance;
            this.rootMotion = rootMotion;
            this.rootMotionMult = rootMotionMult;
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            // if (Actor is AIEnemy aiEnemy)
            // {
            //     ChangeState<IIdleState>();
            //     return;
            // }
            AIEnemy.StopMovement();
            if (rootMotion)
            {
                AIEnemy.SetRootMotionMult(rootMotionMult);
            }

            var distancePatrol = SimpleMath.RandomOnCircleXZ() * patrolMaxDistance;
            targetPosition = AIEnemy.DefaultPos + distancePatrol;
            if (NavMesh.SamplePosition(targetPosition, out var hit, patrolMaxDistance, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                targetPosition = hit.position;
            }
            lastPos = AIEnemy.Position;
            updateLastPosTime = Time.time;
            patrolTime = Random.Range(3f, 5f);
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (AIEnemy.IsPatrolBlockedByTut)
            {
                ChangeState<IIdleState>();
                return;
            }
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    if (stopIfEnemyClose && AIEnemy.IsEnemyClose())
                    {
                        ChangeState<IIdleState>();
                    }
                    else
                    {
                        Patrol();
                    }
                }
                else
                {
                    ChangeState<IIdleState>();    
                }
            }
            else
            {
                
                Patrol();
            }
        }

        void Patrol()
        {
            patrolTime -= Time.deltaTime;
            AIEnemy.UpdateLocomotion(Vector2.up * AnimLocomotionRate, Time.deltaTime);
                        
            if (!rootMotion)
            {
                AIEnemy.MovePosition(targetPosition, Speed, AIEnemy.RotateSpeed, 0.1f);
            }
            else
            {
                var dir = targetPosition - AIEnemy.Position;
                dir.y = 0;
                dir.Normalize();
                AIEnemy.RotateDirection(dir, AIEnemy.RotateSpeed);
            }
                        
            if (patrolTime <= 0 || SimpleMath.InRange(AIEnemy.Position, targetPosition, 0.1f))
            {
                ChangeState<IIdleState>();
            }
            else if (!SimpleMath.InRange(AIEnemy.DefaultPos, AIEnemy.Position, patrolMaxDistance))
            {
                targetPosition = AIEnemy.DefaultPos;
                //ChangeState<IIdleState>();
            }
            else if (Time.time - updateLastPosTime > 0.5f)
            {
                if (SimpleMath.InRange(AIEnemy.Position, lastPos, 0.1f))
                {
                    ChangeState<IIdleState>();
                }
                else
                {
                    updateLastPosTime = Time.time;
                    lastPos = AIEnemy.Position;
                }
            }
        }

        public void RandomTargetPatrol()
        {
            targetPosition = AIEnemy.DefaultPos + SimpleMath.RandomOnCircleXZ() * patrolMaxDistance;
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            AIEnemy.LastTimePatrol = Time.time;
            AIEnemy.SetRootMotionMult(0);
        }
    }
}
