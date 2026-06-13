using System.Collections;
using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/ChaseDurationSkill")]
    public class AIChaseDurationSkill : Skill
    {
        [SerializeField] string chaseAnim;
        [SerializeField] float stoppingDistance;
        [SerializeField] float chaseDuration;
        [SerializeField] ParticleSystem trailFxPrefab;
        [SerializeField] Feedback beginFeedback;

        public override bool IsStopConditionMet => Actor.AimedEnemy is not { Alive: true };

        // Coroutine useRoutine;
        float angle;
        ParticleSystem trailFx;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            Actor.OnSkillBeginAttack();
        }

        protected override IEnumerator IEPerform()
        {
            var chaseRemain = chaseDuration;
            Actor.PlayAnimation(chaseAnim, 0, 1, true, false);
            if (trailFxPrefab)
            {
                trailFx = pools.Spawn(trailFxPrefab, Actor.Transform);
                trailFx.transform.localPosition = trailFxPrefab.transform.localPosition;
                trailFx.transform.localRotation = trailFxPrefab.transform.localRotation;
                trailFx.Play();
            }

            if (beginFeedback)
            {
                beginFeedback.Play();
            }
            
            while (!IsStopConditionMet && chaseRemain > 0)
            {
                chaseRemain -= Time.deltaTime;
                Actor.MovePosition(Actor.AimedEnemy.Position, Actor.RunSpeed, Actor.RotateSpeed, stoppingDistance);
                yield return null;
            }

            if (Actor.AimedEnemy is not { Alive: true })
            {
                if (Actor is Actor.Actor actor)
                {
                    actor.StateMachine.ChangeState<IIdleState>();
                }
            }
            Actor.OnSkillCanMoveNextSkill();
            if (trailFx)
            {
                trailFx.Stop();
                trailFx = null;
            }
        }

        protected override void DoStop()
        {
            base.DoStop();
            Actor.OnSkillEndAttack();
            if (trailFx)
            {
                trailFx.Stop();
                trailFx = null;
            }
        }
    }
}