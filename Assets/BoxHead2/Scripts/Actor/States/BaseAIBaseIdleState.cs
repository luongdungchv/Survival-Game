using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.States
{
    public class BaseAIBaseIdleState : AIBaseState, IIdleState
    {
        protected bool JustGetHit;
        float spawnDelay;
        bool rotating;
        float lastTimeRotate;
        float rotateDuration;
        bool useSpawnSkill;

        public BaseAIBaseIdleState(AIEnemy aiEnemy) : base(aiEnemy)
        {
            useSpawnSkill = false;
            spawnDelay = 0.5f;
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            var currentAnimState = AIEnemy.Animator.GetCurrentAnimatorStateInfo(0);
            var nextAnimState = AIEnemy.Animator.GetNextAnimatorStateInfo(0);
            if (!currentAnimState.IsName("locomotion") && !nextAnimState.IsName("locomotion"))
            {
                AIEnemy.PlayAnimation("locomotion", 0, 1, true, true);
            }

            AIEnemy.UpdateLocomotion(Vector2.zero);
            JustGetHit = Time.time - Actor.LastTimeGetHit < 0.5f;
            AIEnemy.GetHitCount = 0;
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
#if !DACODER_RELEASE || UNITY_EDITOR
            if (AIEnemy.IsInvincible) return;
#endif
            if (!useSpawnSkill && AIEnemy.TryGetSpecialSkill(SpecialSkillId.AfterSpawn, out var list))
            {
                useSpawnSkill = true;
                if (list.Count > 0)
                {
                    AIEnemy.CurrentSkillCombo = new SkillCombo(list.ToArray());
                    StateMachine.ChangeState<AIBaseAttackState>();
                    return;
                }
            }
            else
            {
                useSpawnSkill = true;
            }

            AIEnemy.UpdateLocomotion(Vector2.zero, Time.deltaTime);
            if (spawnDelay > 0)
            {
                spawnDelay -= Time.deltaTime;
            }
            else
            {
                StateUpdate();
            }
        }

        protected virtual void StateUpdate()
        {
        }

        protected bool RotateToTarget()
        {
            var angle = Vector3.Angle(AIEnemy.ForwardDirection, AIEnemy.AimedDirection);
            if (!rotating && angle > 30f && Time.time - lastTimeRotate > 0.5f)
            {
                rotating = true;
                rotateDuration = 0;
            }

            if (rotating && (angle < 10f || rotateDuration > 0.5f))
            {
                rotating = false;
                lastTimeRotate = Time.time;
            }

            if (rotating)
            {
                rotateDuration += Time.deltaTime;
                AIEnemy.UpdateLocomotion(Vector2.up * 0.5f, Time.deltaTime);
                AIEnemy.RotateDirection(AIEnemy.AimedDirection, AIEnemy.RotateSpeed);
            }
            else
            {
                AIEnemy.UpdateLocomotion(Vector2.zero, Time.deltaTime);
            }

            return rotating;
        }
    }
}