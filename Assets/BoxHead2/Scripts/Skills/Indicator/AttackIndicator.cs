using System;
using Dacodelaac.Core;
using Dacodelaac.ObjectPooling;
using UnityEngine;

namespace BoxHead2.Skills
{
    public abstract class AttackIndicator : BaseMono
    {
        [SerializeField] protected Feedback visibleFeedback;
        public abstract void Setup(AttackIndicatorData attackIndicatorData);
        public abstract void DoUpdate(Transform source, Vector3 targetPos, Vector3 targetDirection, float range, float radius);
    }

    [Serializable]
    public class AttackIndicatorData
    {
        [SerializeField] AttackIndicator attackIndicatorPrefab;
        [SerializeField] public UpdateRadius updateRadius = UpdateRadius.Runtime;
        [SerializeField] public float radius;
        [SerializeField] public UpdatePosition updatePosition = UpdatePosition.FollowTarget;
        [SerializeField] public Vector3 positionOffset;
        [SerializeField] public Vector3 rotationOffset;

        public AttackIndicator Spawn(Pools pools)
        {
            if (attackIndicatorPrefab)
            {
                var attackIndicator = pools.Spawn(attackIndicatorPrefab);
                attackIndicator.Setup(this);
                return attackIndicator;
            }

            return null;
        }

        public enum UpdateRadius
        {
            Fixed,
            Runtime
        }
        
        public enum UpdatePosition
        {
            FollowSource,
            FollowTarget,
        }
    }
}