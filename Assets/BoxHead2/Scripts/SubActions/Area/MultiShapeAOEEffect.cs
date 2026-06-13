using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Skills;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public class MultiShapeAOEEffect : AreaOfEffect
    {
        [SerializeField] HurtBox[] hurtBoxes;
        List<HitScanData> hits = new ();
        protected override void DealDamage()
        {
            hits ??= new List<HitScanData>();
            hits.Clear();
            if(hurtBoxes != null && hurtBoxes.Length > 0) hurtBoxes[0].Group.DamageTakers.Clear();
            foreach (var hurtBox in hurtBoxes)
            {
                hits.AddRange(hurtBox.Scan(damageLayer));
            }

            bool dmgDealt = false;
            foreach (var hit in hits)
            {
                var position = Transform.position;
                
                //Get index tick
                var damageConfig = GetDamageConfig(tickCount);
                var combinedDamage = CombinedDamageData.Combine(SourceData, damageConfig);
                combinedDamage.Damage *= hit.DamageScale;
                combinedDamage.UpdateDamageForce(Vector3.zero, hit.HitPosition, position);
                if (hit.OverrideDamageForce)
                {
                    combinedDamage.OverrideDamageForce(hit.DamageForce);
                }

                var hitType = hit.DamageTaker.TakeDamage(combinedDamage);
                hitFeedback.Play(hitType.HitType);
                OnDealDamage(hit.DamageTaker, hitType.HitType);
            }
        }
    }
}