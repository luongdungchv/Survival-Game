using System;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class GroundAoeIndicator : AttackIndicator
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
                if (!particleSystem.isPlaying) particleSystem.Play();
            }
            switch (data.updateRadius)
            {
                case AttackIndicatorData.UpdateRadius.Fixed:
                    transform.localScale = Vector3.one * data.radius;
                    break;
                case AttackIndicatorData.UpdateRadius.Runtime:
                    transform.localScale = Vector3.one * radius;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (data.updatePosition)
            {
                case AttackIndicatorData.UpdatePosition.FollowSource:
                    targetPos = source.TransformPoint(data.positionOffset);
                    targetPos.y += 0.1f;
                    transform.position = targetPos;
                    break;
                case AttackIndicatorData.UpdatePosition.FollowTarget:
                    targetPos += source ? source.TransformDirection(data.positionOffset) : data.positionOffset;
                    targetPos.y += 0.1f;
                    transform.position = targetPos;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}