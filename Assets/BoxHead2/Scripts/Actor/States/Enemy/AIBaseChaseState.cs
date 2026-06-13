using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseChaseState : AIBaseState
    {
        float delay;
        bool rootMotion;
        float rootMotionMult;
        
        public AIBaseChaseState(AIEnemy aiEnemy, bool rootMotion = false, float rootMotionMult = 1) : base(aiEnemy)
        {
            this.rootMotion = rootMotion;
            this.rootMotionMult = rootMotionMult;
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            if (rootMotion)
            {
                AIEnemy.SetRootMotionMult(rootMotionMult);
            }
            delay = 0.5f;
        }
        
        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (delay > 0)
            {
                AIEnemy.UpdateLocomotion(Vector2.zero, Time.deltaTime);
                delay -= Time.deltaTime;
                return;
            }
            
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    ChangeState<IIdleState>();
                }
                else
                {
                    if (!AIEnemy.IsEnemyInAttackRange(-0.2f) && !AIEnemy.IsEnemyClose())
                    {
                        var skill = AIEnemy.GetCurrentOrNextSkill();
                        var stoppingDistance = skill ? skill.Range - 0.2f : AIEnemy.EnemyCloseRange;
                        
                        if (!rootMotion)
                        {
                            AIEnemy.MovePosition(AIEnemy.AimedEnemy.Position, AIEnemy.RunSpeed, AIEnemy.RotateSpeed, stoppingDistance);
                        }
                        else
                        {
                            AIEnemy.RotateDirection(AIEnemy.AimedDirection, AIEnemy.RotateSpeed);
                        }
                        AIEnemy.UpdateLocomotion(Vector2.up, Time.deltaTime);
                    }
                    else
                    {
                        ChangeState<IIdleState>();
                    }
                }
            }
            else
            {
                ChangeState<IIdleState>();
            }
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            AIEnemy.SetRootMotionMult(0);
        }
    }
}