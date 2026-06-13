using System;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class DirectionAttackParticleIndicator : AttackIndicator
    {
        [SerializeField] ParticleSystem particleSystem;
        AttackIndicatorData data;
        
        public override void Setup(AttackIndicatorData data)
        {
            this.data = data;
            gameObject.SetActive(false);
            if (particleSystem != null)
            {
                particleSystem.Clear();
            }
        }

        public override void DoUpdate(Transform source, Vector3 targetPos, Vector3 targetDirection, float range, float radius)
        {
            gameObject.SetActive(true);
            if (particleSystem)
            {
                if (!particleSystem.isPlaying)
                {
                    if (visibleFeedback)
                    {
                        visibleFeedback.Play();
                    }
                    particleSystem.Play();
                }
            }
            switch (data.updateRadius)
            {
                case AttackIndicatorData.UpdateRadius.Fixed:
                    transform.localScale = new Vector3(data.radius, 1f, 1f);
                    break;
                case AttackIndicatorData.UpdateRadius.Runtime:
                    transform.localScale = new Vector3(radius, 1f, 1f);;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (data.updatePosition)
            {
                case AttackIndicatorData.UpdatePosition.FollowSource:
                    targetPos = source.TransformPoint(data.positionOffset);
                    transform.position = targetPos;
                    break;
                case AttackIndicatorData.UpdatePosition.FollowTarget:
                    targetPos += source ? source.TransformDirection(data.positionOffset) : data.positionOffset;
                    transform.position = targetPos;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Transform.rotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(data.rotationOffset);
        }
    }
}