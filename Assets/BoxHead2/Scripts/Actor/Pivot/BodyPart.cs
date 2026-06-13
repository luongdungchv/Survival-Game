using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class BodyPart : BaseMono
    {
        [SerializeField] public BodyPartType bodyPartType;

        Collider _collider;
        public Collider Collider => gameObject.GetAndCacheComponent(ref _collider);

        public override void Initialize()
        {
            base.Initialize();
            Collider.enabled = true;
            Collider.isTrigger = true;
        }

        public void SetLayer(int hitBoxLayer)
        {
            gameObject.layer = hitBoxLayer;
        }

        public void TurnOffHitBox()
        {
            Collider.enabled = false;
        }

        public void TurnOnHitBox()
        {
            Collider.enabled = true;
        }
    }

    public enum BodyPartType
    {
        Body,
        Head,
        Leg,
        Arm
    }
}