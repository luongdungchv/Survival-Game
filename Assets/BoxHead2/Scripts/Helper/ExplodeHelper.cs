using System;
using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using Dacodelaac.DebugUtils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoxHead2.Helper
{
    public static class ExplodeHelper
    {
        public static void Scan(Vector3 pos, float radius, CombinedDamageData combinedDamageData, LayerMask damageLayer,
            System.Action<Vector3, Vector3, HitType> onHit, System.Action<float> onDealDamage = null)
        {
#if UNITY_EDITOR
            Dacoder.DrawWireSphere(pos, radius, Color.red, 1f, 1);
#endif
            var hashSet = new HashSet<IDamageTaker>();
            var hitResults = Physics.OverlapSphere(pos, radius, damageLayer);
            var totalDamageDeal = 0f;
            if (hitResults != null)
            {
                foreach (var hit in hitResults)
                {
                    var damageTaker = hit.GetComponentInParent<IDamageTaker>();
                    if (damageTaker is { Alive: true } && !hashSet.Contains(damageTaker))
                    {
                        var dir = damageTaker.Position - pos;
                        dir.y = 0;
                        if (dir == Vector3.zero)
                        {
                            dir = Random.onUnitSphere;
                            dir.y = 0;
                        }

                        dir.Normalize();
                        combinedDamageData.UpdateDamageForce(dir, hit.transform.position, pos);
                        hashSet.Add(damageTaker);
                        var hitType = damageTaker.TakeDamage(combinedDamageData);
                        onHit?.Invoke(dir, hit.transform.position, hitType.HitType);
                        totalDamageDeal += hitType.Damage;
                    }
                }
            }

            onDealDamage?.Invoke(totalDamageDeal);
        }

        public static void ScanHealAoe(Vector3 pos, float radius, LayerMask healLayer, float damage, HealType healType)
        {
#if UNITY_EDITOR
            Dacoder.DrawWireSphere(pos, radius, Color.red, 1f, 1);
#endif
            var hashSet = new HashSet<IDamageTaker>();
            var hitResults = Physics.OverlapSphere(pos, radius, healLayer);
            var amount = healType switch
            {
                HealType.BaseOnDamage => damage,
                _ => 0
            };
            var healData = new HealData
            {
                ActiveFx = true,
                Amount = amount,
                HpText = true,
                IsPlaySound = false,
                ReviveText = false,
                Revive = false
            };
            foreach (var hit in hitResults)
            {
                var damageTaker = hit.GetComponentInParent<IDamageTaker>();
                if (damageTaker is { Alive: true } && !hashSet.Contains(damageTaker))
                {
                    damageTaker.Heal(healData);
                    hashSet.Add(damageTaker);
                }
            }
        }
        
        public static void ScanShockWaveReflect(Vector3 pos, float radius, float trueDamage, LayerMask damageLayer, ActorType sourceType)
        {
#if UNITY_EDITOR
            Dacoder.DrawWireSphere(pos, radius, Color.red, 1f, 1);
#endif
            var hashSet = new HashSet<IDamageTaker>();
            var hitResults = Physics.OverlapSphere(pos, radius, damageLayer);
            foreach (var hit in hitResults)
            {
                var damageTaker = hit.GetComponentInParent<IDamageTaker>();
                if (damageTaker != null && damageTaker.Alive && !hashSet.Contains(damageTaker))
                {
                    var dir = damageTaker.Position - pos;
                    dir.y = 0;
                    if (dir == Vector3.zero)
                    {
                        dir = Random.onUnitSphere;
                        dir.y = 0;
                    }

                    dir.Normalize();
                    hashSet.Add(damageTaker);
                    damageTaker.ApplyTrueDamage(trueDamage, sourceType);
                }
            }
        }

        public static List<IDamageTaker> ScanShockWave(Vector3 pos, float radius, float radiusInside,
            CombinedDamageData combinedDamageData, LayerMask damageLayer, HashSet<IDamageTaker> exclude)
        {
#if UNITY_EDITOR
            Dacoder.DrawWireSphere(pos, radius, Color.red, 1f, 1);
#endif
            var hitResults = Physics.OverlapSphere(pos, radius, damageLayer);
            var results = new List<IDamageTaker>();
            foreach (var hit in hitResults)
            {
                var damageTaker = hit.GetComponentInParent<IDamageTaker>();
                if (damageTaker != null && damageTaker.Alive && !exclude.Contains(damageTaker))
                {
                    var dir = damageTaker.Position - pos;
                    dir.y = 0;
                    if (dir.sqrMagnitude < radiusInside * radiusInside)
                    {
                        continue;
                    }

                    if (dir == Vector3.zero)
                    {
                        dir = Random.onUnitSphere;
                        dir.y = 0;
                    }

                    dir.Normalize();
                    combinedDamageData.UpdateDamageForce(dir, hit.transform.position, pos);
                    exclude.Add(damageTaker);
                    results.Add(damageTaker);
                    damageTaker.TakeDamage(combinedDamageData);
                }
            }

            return results;
        }
    }
}