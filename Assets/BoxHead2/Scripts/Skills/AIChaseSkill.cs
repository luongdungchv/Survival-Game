using System.Collections;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/ChaseSkill")]
    public class AIChaseSkill : Skill
    {
        [SerializeField] string chaseAnim;
        [SerializeField] float stoppingDistance;
        [SerializeField] ParticleSystem trailFxPrefab;
        [Header("Feedback")] [SerializeField] Feedback beginFeedback;

        public override bool IsStopConditionMet => Actor.AimedEnemy == null;

        // Coroutine useRoutine;
        float angle;
        ParticleSystem trailFx;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            Actor.OnSkillBeginAttack();
            // useRoutine = Actor.StartCoroutine(IEUse());
        }

        protected override IEnumerator IEPerform()
        {
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

            while (!IsStopConditionMet && Actor.GetEnemyDistance() > stoppingDistance)
            {
                Actor.MovePosition(Actor.AimedEnemy.Position, Actor.RunSpeed, Actor.RotateSpeed, stoppingDistance);
                yield return null;
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