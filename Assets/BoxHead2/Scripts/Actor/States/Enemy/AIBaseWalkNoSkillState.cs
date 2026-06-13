using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseWalkNoSkillState : AIBaseState
    {
        float delay;
        bool rootMotion;
        float rootMotionMult;
        
        public AIBaseWalkNoSkillState(AIEnemy aiEnemy, bool rootMotion = false, float rootMotionMult = 1) : base(aiEnemy)
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
                    if (AIEnemy.IsEnemyClose())
                    {
                        ChangeState<IIdleState>();
                    }
                    else
                    {
                        if (!rootMotion)
                        {
                            AIEnemy.MovePosition(AIEnemy.AimedEnemy.Position, AIEnemy.WalkSpeed, AIEnemy.RotateSpeed, AIEnemy.EnemyCloseRange);
                        }
                        else
                        {
                            AIEnemy.RotateDirection(AIEnemy.AimedDirection, AIEnemy.RotateSpeed);
                        }
                        AIEnemy.UpdateLocomotion(Vector2.up * 0.5f, Time.deltaTime);
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

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            AIEnemy.SetRootMotionMult(0);
        }
    }
}