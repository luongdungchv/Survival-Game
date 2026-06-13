using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseWalkChaseState : AIBaseState
    {
        bool rootMotion;
        private bool checkSkillRange;
        public AIBaseWalkChaseState(AIEnemy aiEnemy, bool rootMotion = false, bool checkSkillRange = true) : base(aiEnemy)
        {
            this.rootMotion = rootMotion;
            this.checkSkillRange = checkSkillRange;
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            if (rootMotion)
            {
                AIEnemy.SetRootMotionMult(1);
            }
        }
        
        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    ChangeState<IIdleState>();
                }
                else
                {
                    if (!AIEnemy.IsEnemyInAttackRange(-0.2f) && !AIEnemy.IsEnemyClose(checkSkillRange))
                    {
                        var skill = AIEnemy.GetCurrentOrNextSkill();
                        var stoppingDistance = skill ? skill.Range - 0.2f : AIEnemy.EnemyCloseRange;
                        if (!rootMotion)
                        {
                            AIEnemy.MovePosition(AIEnemy.AimedEnemy.Position, AIEnemy.WalkSpeed, AIEnemy.RotateSpeed, stoppingDistance);
                        }
                        else
                        {
                            AIEnemy.RotateDirection(AIEnemy.AimedDirection, AIEnemy.RotateSpeed);
                        }
                        
                        AIEnemy.UpdateLocomotion(Vector2.up * 0.5f, Time.deltaTime);
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