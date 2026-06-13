using System.Collections.Generic;
using System.Linq;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Skills;
using BoxHead2.SubCombat;
using Dacodelaac.DebugUtils;
using Dacodelaac.ObjectPooling;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Helper
{
    public static class HurtBoxHelper
    {
        public static void SetupHurtBoxes(Pools pools, IActor actor, HurtBoxGroup[] hurtBoxGroupPrefabs, out HurtBoxGroup[] hurtBoxGroups,
            out HurtBox[] hurtBoxes)
        {
            hurtBoxGroups = new HurtBoxGroup[hurtBoxGroupPrefabs.Length];
            var hurtBoxesList = new List<HurtBox>();
            for (var i = 0; i < hurtBoxGroupPrefabs.Length; i++)
            {
                hurtBoxGroups[i] = pools.Spawn(hurtBoxGroupPrefabs[i], actor.Transform);
                hurtBoxGroups[i].transform.ResetLocal();
                hurtBoxesList.AddRange(hurtBoxGroups[i].GetComponentsInChildren<HurtBox>());
            }

            hurtBoxes = hurtBoxesList.ToArray();
        }

        public static void DespawnHurtBoxes(Pools pools, ref HurtBoxGroup[] hurtBoxGroups)
        {
            if (hurtBoxGroups != null)
            {
                foreach (var hurtBoxGroup in hurtBoxGroups)
                {
                    if (hurtBoxGroup)
                    {
                        pools.Despawn(hurtBoxGroup.gameObject);
                    }
                }
            }

            hurtBoxGroups = null;
        }

        public static void ExecuteFrameBaseHurtBox(IActor actor, LayerMask layer, string animation, AnimationClip clip, HurtBox[] hurtBoxes,
            CombinedDamageData combinedDamageData, bool clearScanned, float extraDamageFrame, ref float currentFrame, ref float lastFrame,
            System.Action<Vector3, Vector3, HitType, IDamageTaker> onHit = null)
        {
            if (clearScanned)
            {
                for (var i = 0; i < hurtBoxes.Length; i++)
                {
                    hurtBoxes[i].Group.DamageTakers.Clear();
                }
            }
            var state = actor.Animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(animation) && !actor.Animator.IsInTransition(0))
            {
                currentFrame = clip.length * clip.frameRate * state.normalizedTime;
                for (var i = 0; i < hurtBoxes.Length; i++)
                {
                    if (hurtBoxes[i].startFrame != -1 && hurtBoxes[i].endFrame != -1 &&
                        currentFrame >= hurtBoxes[i].startFrame &&
                        lastFrame <= hurtBoxes[i].endFrame + extraDamageFrame)
                    {
                        actor.DealingDamage = true;
                        ExecuteHurtBox(actor.Position, actor.Transform.forward, layer, hurtBoxes[i], combinedDamageData, onHit);
                    }
                }

                lastFrame = currentFrame;
            }
        }

        public static bool ExecuteManualHurtBoxCheck(LayerMask layer, HurtBox[] hurtBoxes)
        {
            foreach (var hurtBox in hurtBoxes)
            {
                if (hurtBox.ScanCheck(layer))
                {
                    return true;
                }
            }

            return false;
        }

        public static void ExecuteManualHurtBox(Vector3 position, Vector3 direction, LayerMask layer, CombinedDamageData combinedDamageData, 
            bool clearScanned, HurtBox[] hurtBoxes,System.Action<Vector3, Vector3, HitType, IDamageTaker> onHit = null, IDamageTaker exclude = null)
        {
            if (clearScanned)
            {
                for (var i = 0; i < hurtBoxes.Length; i++)
                {
                    hurtBoxes[i].Group.DamageTakers.Clear();
                }
            }
            for (var i = 0; i < hurtBoxes.Length; i++)
            {
                ExecuteHurtBox(position, direction, layer, hurtBoxes[i], combinedDamageData, onHit, exclude);
            }
        }

        static void ExecuteHurtBox(Vector3 position, Vector3 direction, LayerMask layer, HurtBox hurtBox, CombinedDamageData combinedDamageData,
            System.Action<Vector3, Vector3, HitType, IDamageTaker> onHit = null, IDamageTaker exclude = null)
        {
            var hitDirection = Vector3.zero;
            if (hurtBox)
            {
                if (!hurtBox.explosionForce)
                {
                    hitDirection = direction.normalized;
                }

                var hits = hurtBox.Scan(layer);
                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    if (exclude == hit.DamageTaker) continue;
                    if (hit.DamageTaker == null) continue;

                    var hitPosition = hit.HitPosition;
                    if (hurtBox.explosionForce)
                    {
                        var dir = hit.DamageTaker.Position - position;
                        dir.y = 0;
                        dir.Normalize();
                        if (dir == Vector3.zero)
                        {
                            dir = direction;
                        }

                        hitDirection = dir;
                    }

                    var combinedDamage = combinedDamageData;
                    combinedDamage.Damage *= hit.DamageScale;
                    combinedDamage.UpdateDamageForce(hitDirection, hitPosition, position);
                    if (hit.OverrideDamageForce)
                    {
                        combinedDamage.OverrideDamageForce(hit.DamageForce);
                    }

                    var hitType = hit.DamageTaker.TakeDamage(combinedDamage);
                    onHit?.Invoke(hit.HitColliderPosition, hitDirection, hitType.HitType, hit.DamageTaker);
                }
            }
        }

        public static List<HitScanData> Scan(Vector3 pos, float radius, Collider[] hitResults,
            HashSet<IDamageTaker> damageTakerSet, LayerMask layerMask)
        {
            Dacoder.DrawWireSphere(pos, radius, Color.red, 0.1f, 1);

            var results = new List<HitScanData>();
            Debug.LogError((1, layerMask));
            var count = Physics.OverlapSphereNonAlloc(pos, radius, hitResults, layerMask);
            Debug.LogError((2, count));
            for (var i = 0; i < count; i++)
            {
                Debug.LogError(hitResults[i]);
                var damageTaker = hitResults[i].GetComponentInParent<IDamageTaker>();
                if (damageTakerSet.Contains(damageTaker))
                {
                    continue;
                }
                
                damageTakerSet.Add(damageTaker);
                if (!damageTaker.IsRealNull() && damageTaker is {Alive: true, Initialized: true})
                {
                    results.Add(new HitScanData(damageTaker, hitResults[i].transform.position,
                        hitResults[i].ClosestPoint(pos), 1, false, new DamageForceData()));
                }
                hitResults[i].GetComponent<ColliderHitFx>()?.OnHit(pos, Vector3.zero);
            }
            
            return results;
        }

        public static bool ScanCheck(Vector3 pos, float radius, Collider[] hitResults, LayerMask layerMask)
        {
            Dacoder.DrawWireSphere(pos, radius, Color.red, 0.1f, 1);

            var count = Physics.OverlapSphereNonAlloc(pos, radius, hitResults, layerMask);
            return count > 0;
        }

        public static void SwitchDamageLayer(ref LayerMask damageLayer)
        {
            damageLayer &= ~(1 << LayerMask.NameToLayer("Hurbox_1"));
            damageLayer |= 1 << LayerMask.NameToLayer("Hurbox_2");
        }
        
        public static void SwitchHitBoxLayer(ref LayerMask hitBoxLayer)
        {
            hitBoxLayer &= ~(1 << LayerMask.NameToLayer("Hitbox_2"));
            hitBoxLayer |= 1 << LayerMask.NameToLayer("Hitbox_1");
        }
    }
}