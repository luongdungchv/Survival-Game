using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileHitAction")]
    public class MissileHitAction : SubAction
    {
        [SerializeField] int basePenetrateTime;
        [SerializeField] float penetrateDamageBonus;
        [SerializeField] bool despawnOnHit = true;
        [Header("Damage")] 
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.DamageLayer;
        [SerializeField] DamageConfig damageConfig;
        [Header("Feedback")]
        [SerializeField] HitFeedback hitFeedback;
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] bool isSpawnDefaultToTaker;

        protected HashSet<IDamageTaker> _damageTakerSet = new ();
        Missile _missile;
        int _penetrate;
        
        protected virtual DamageConfig DamageConfig => damageConfig;
        public override void Trigger(object target, IActor actor)
        {
            var data = Get<MissileHitActionData>(target);
            if (data.DamageTaker == null)
            {
                if (despawnOnHit)
                {
                    _missile.Despawn();
                }
                return;
            }
            if (_damageTakerSet.Contains(data.DamageTaker)) return;
            
            _damageTakerSet.Add(data.DamageTaker);
            
            var sourceData = data.SourceData;
            var combinedDamage = CombinedDamageData.Combine(sourceData, DamageConfig);
            combinedDamage.UpdateDamageForce(data.HitDirection, data.HitPosition, _missile.Transform.position);

            var hitType = data.DamageTaker.TakeDamage(combinedDamage);
            if (hitFxPrefab)
            {
                if (isSpawnDefaultToTaker)
                {
                    FxHelper.SpawnFxAttachTransform(pools, data.DamageTaker.Transform, hitFxPrefab, null);
                }
                else
                {
                    FxHelper.SpawnFx(pools, data.HitPosition, _missile.Transform.forward, hitFxPrefab, null);
                }
            }

            hitFeedback?.Play(hitType.HitType);

            if (data.DamageTaker is { Alive: false })
            {
                OnKilledEnemies(data.SourceData.DamageSource);
            }

            if (_penetrate > 0)
            {
                _penetrate--;
                sourceData.DamageBonus += penetrateDamageBonus;
                _missile.SourceData = sourceData;
            }
            else if (despawnOnHit)
            {
                if (_missile) _missile.Despawn();
            }
        }

        protected virtual void OnKilledEnemies(IDamageSource damageSource)
        {
            
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            _missile = data.Missile;
            _missile.gameObject.layer = data.SetupData.Actor.TeamConfig.GetLayerMask(damageLayer).ToGameObjectLayer();
            _damageTakerSet = new HashSet<IDamageTaker>();
            _penetrate = basePenetrateTime;
        }
    }
    
    public struct MissileHitActionData
    {
        public IDamageTaker DamageTaker { get; }
        public Vector3 HitPosition { get; }
        public Vector3 HitDirection { get; }
        public Vector3 HitNormal { get; }
        public DamageSourceData SourceData { get; }

        public MissileHitActionData(IDamageTaker damageTaker, Vector3 hitPosition, Vector3 hitDirection, Vector3 hitNormal,
            DamageSourceData sourceData)
        {
            DamageTaker = damageTaker;
            HitPosition = hitPosition;
            HitDirection = hitDirection;
            SourceData = sourceData;
            HitNormal = hitNormal;
        }
    }
}