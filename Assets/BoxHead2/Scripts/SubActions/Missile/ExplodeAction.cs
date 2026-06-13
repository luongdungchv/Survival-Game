using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Skills;
using DG.Tweening;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/ExplodeAction")]
    public class ExplodeAction : SubAction
    {
        [SerializeField] float radius;
        [SerializeField] bool scaleFx = true;
        [Header("Damage")]
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] DamageConfig damageConfig;
        [Header("Feedback")]
        [SerializeField] ParticleSystem explodeFxPrefab;
        [SerializeField] Feedback explodeFeedback;
        [SerializeField] HitFeedback hitFeedback;
        [SerializeField] float indicatorDuration = 0.5f;
        [SerializeField] AttackIndicatorData indicatorData;

        [SerializeField] bool isExplodeGround;
        [SerializeField] LayerMask ground;

        [Header("Delay")] 
        [SerializeField] float delayTime = 0;

        public float Radius => _data is {IsOverrideRadius: true} ? _data.OverrideRadius : (radius);

        ExplodeActionData _data;
        DamageSourceData _sourceData;
        Vector3 _explodePosition;
        AttackIndicator _indicator;
        IActor _actor;

        public override void Trigger(object target, IActor actor)
        {
            _data = Get<ExplodeActionData>(target);
            _sourceData = _data.SourceData;
            _explodePosition = _data.Position;
            this._actor = actor;
            if (delayTime <= 0)
            {
                Explode();
                ExplodeDamage();
            }
            else
            {
                StartPerformRoutine();
            }

            DespawnWarning();
        }

        public override void Prepare(object target)
        {
            _indicator = indicatorData.Spawn(pools);
            if (_indicator)
            {
                _data = Get<ExplodeActionData>(target);
                DOTween.To(() => 0f,
                    x =>
                    {
                        _indicator.DoUpdate(null, _data.WarningPosition, Vector3.zero, 0, Radius * x);
                    }, 1f, indicatorDuration).SetTarget(_indicator).Play();
            }
        }

        void DespawnWarning()
        {
            if (_indicator)
            {
                DOTween.Kill(_indicator);
                pools.Despawn(_indicator.gameObject);
                _indicator = null;
            }
        }

        void Explode()
        {
            //if (explodePosition.y < 0.1f) explodePosition.y = 0.1f;
            
            if (isExplodeGround &&
                Physics.Raycast(_explodePosition + Vector3.up, Vector3.down * 10f, out var hit, 10, ground))
            {
                _explodePosition.y = hit.point.y + 0.1f;
            }
            
            FxHelper.SpawnExplodeFx(pools, _explodePosition, Vector3.zero, Radius, scaleFx, explodeFxPrefab, explodeFeedback, _actor);
        }

        void ExplodeDamage()
        {
            var damage = this.damageConfig;
            if (_data.UseCustomDamage)
            {
                damage.damage *= _data.CustomDamage.damage;
                damage.staggerChance += _data.CustomDamage.staggerChance;
                damage.slowChance += _data.CustomDamage.slowChance;
            }
            var combinedDamage = CombinedDamageData.Combine(_sourceData, damage);
            
            ExplodeHelper.Scan(_explodePosition, Radius, combinedDamage, _actor.TeamConfig.GetLayerMask(damageLayer), (pos, dir, hitType) =>
            {
                hitFeedback.Play(hitType);
            });
        }

        protected override IEnumerator IEPerform()
        {
            Explode();
            yield return new WaitForSeconds(delayTime);
            ExplodeDamage();
        }
    }
    
    public struct ExplodeActionData
    {
        public Vector3 Position;
        public float RadiusBonus;
        public DamageSourceData SourceData;
        public DamageConfig CustomDamage;
        public bool UseCustomDamage;
        public Vector3 WarningPosition;
        public bool IsOverrideRadius;
        public float OverrideRadius;
        public ExplodeActionData(Vector3 position, Vector3 warningPosition, float radiusBonus, DamageSourceData sourceData, DamageConfig customDamage, bool useCustomDamage, bool isOverrideRadius = false, float overrideRadius = -1)
        {
            Position = position;
            WarningPosition = warningPosition;
            RadiusBonus = radiusBonus;
            SourceData = sourceData;
            CustomDamage = customDamage;
            UseCustomDamage = useCustomDamage;
            IsOverrideRadius = isOverrideRadius;
            OverrideRadius = overrideRadius;
        }
    }
}