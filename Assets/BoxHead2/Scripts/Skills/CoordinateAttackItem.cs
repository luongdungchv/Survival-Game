using System.Collections.Generic;
using BoxHead2.Combat;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Skills
{
    public abstract class CoordinateAttackItem : BaseMono
    {
        [SerializeField] HurtBox[] hurtBoxes;
        [SerializeField] Feedback triggerFeedback;
        [SerializeField] bool perHurtBoxTrigger;

        protected LayerMask damageLayer;
        protected CombinedDamageData combinedDamageData;

        public float Radius
        {
            get
            {
                var result = 0f;
                foreach(var h in hurtBoxes) result += h.radius;
                result /= hurtBoxes.Length;
                return result;
            }
        }

        public void SetDamageLayer(LayerMask damageLayer)
        {
            this.damageLayer = damageLayer;
        }

        public void SetDamageData(CombinedDamageData combinedDamageData)
        {
            this.combinedDamageData = combinedDamageData;
        }

        public abstract void StartDealingDamage();

        protected void DealDamage(int index)
        {
            if (perHurtBoxTrigger)
            {
                var targetHurtbox = hurtBoxes[index];
                targetHurtbox.Group.DamageTakers.Clear();
                var hits = targetHurtbox.Scan(damageLayer);
                foreach (var hit in hits)
                {
                    hit.DamageTaker.TakeDamage(combinedDamageData);
                }
            }
            else
            {
                var groupList = new HashSet<HurtBoxGroup>();
                foreach (var hurtBox in hurtBoxes)
                {
                    groupList.Add(hurtBox.Group);
                }

                foreach (var group in groupList)
                {
                    group.DamageTakers.Clear();
                }

                foreach (var hurtBox in hurtBoxes)
                {
                    var hits = hurtBox.Scan(damageLayer);
                    foreach (var hit in hits)
                    {
                        var position = Transform.position;
                        var direction = hurtBox.explosionForce ? hit.DamageTaker.Position - position : Transform.forward;
                        direction.y = 0;
                        direction.Normalize();
                        combinedDamageData.UpdateDamageForce(direction, hit.HitPosition, position);
                        hit.DamageTaker.TakeDamage(combinedDamageData);
                    }
                }
            }

            triggerFeedback?.Play();
        }
    }
}