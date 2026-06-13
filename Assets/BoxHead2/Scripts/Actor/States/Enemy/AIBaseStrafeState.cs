using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class AIBaseStrafeState : AIBaseState
    {
        float lastTimeChangeDirection;
        float strafeAngle;
        
        public AIBaseStrafeState(AIEnemy aiEnemy) : base(aiEnemy)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            if (Time.time - lastTimeChangeDirection > 3f)
            {
                lastTimeChangeDirection = Time.time;
                strafeAngle = Random.value < 0.5f ? 90 : -90;   
            }
        }
        
        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            
            if (AIEnemy.AimedEnemy != null)
            {
                if (!AIEnemy.IsAttackTimeMatch)
                {
                    var dir = AIEnemy.AimedDirection;
                    dir = Quaternion.Euler(0, strafeAngle, 0) * dir;
                    AIEnemy.MoveDirection(dir, AIEnemy.WalkSpeed);
                    AIEnemy.RotateDirection(dir, AIEnemy.RotateSpeed);
                    AIEnemy.UpdateLocomotion(Vector2.up * 0.5f, Time.deltaTime);
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