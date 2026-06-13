using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class HurtBox: BaseMono
    {
        [SerializeField] public float radius;
        [SerializeField] float damageScale = 1;
        [SerializeField] float insideRadiusRatio = -1;
        [SerializeField] float insideDamageScale = 1;
        [SerializeField] bool overrideInsideDamageForce;
        [SerializeField] DamageForceData insideDamageForce;
        [SerializeField] public bool explosionForce;
        [SerializeField] public int startFrame = -1;
        [SerializeField] public int endFrame = -1;

        public HurtBoxGroup Group { get; set; }
        Collider[] hitResults;
        float overrideRadius;
        public float Radius => radius;

        public override void Initialize()
        {
            base.Initialize();
            hitResults = new Collider[20];
            overrideRadius = -1;
        }

        public void OverrideRadius(float value)
        {
            overrideRadius = value;
        }

        public List<HitScanData> Scan(LayerMask layerMask)
        {
            var pos = Transform.position;
            var r = overrideRadius > 0 ? overrideRadius : radius;
            var results = HurtBoxHelper.Scan(pos, r, hitResults, Group.DamageTakers, layerMask);
            foreach (var result in results)
            {
                result.DamageScale = damageScale;
                if (insideRadiusRatio > 0)
                {
                    if (SimpleMath.InRange(result.DamageTaker.Position, pos, insideRadiusRatio * r))
                    {
                        result.DamageScale = insideDamageScale;
                        result.OverrideDamageForce = overrideInsideDamageForce;
                        result.DamageForce = insideDamageForce;
                    }
                }
            }
            return results;
        }

        public bool ScanCheck(LayerMask layerMask)
        {
            var pos = Transform.position;
            var r = overrideRadius > 0 ? overrideRadius : radius;
            return HurtBoxHelper.ScanCheck(pos, r, hitResults, layerMask);
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                var group = GetComponentInParent<HurtBoxGroup>();
                if (group)
                {
                    foreach (var hurtBox in group.GetComponentsInChildren<HurtBox>())
                    {
                        Gizmos.DrawWireSphere(hurtBox.Transform.position, hurtBox.radius);
                    }
                }
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Transform.position, radius);
                if (insideRadiusRatio > 0)
                {
                    Gizmos.DrawWireSphere(Transform.position, radius * insideRadiusRatio);
                }
            }
        }
    }

    public class HitScanData
    {
        public IDamageTaker DamageTaker;
        public Vector3 HitPosition;
        public Vector3 HitColliderPosition;
        public float DamageScale;
        public bool OverrideDamageForce;
        public DamageForceData DamageForce;

        public HitScanData(IDamageTaker damageTaker, Vector3 hitPosition, Vector3 hitColliderPosition, float damageScale, bool overrideDamageForce, DamageForceData damageForce)
        {
            DamageTaker = damageTaker;
            HitPosition = hitPosition;
            HitColliderPosition = hitColliderPosition;
            DamageScale = damageScale;
            OverrideDamageForce = overrideDamageForce;
            DamageForce = damageForce;
        }
    }
}