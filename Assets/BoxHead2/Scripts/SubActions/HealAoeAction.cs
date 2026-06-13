using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/HealAoeAction")]
    public class HealAoeAction : SubAction
    {
        [SerializeField] HealType healType = HealType.BaseOnDamage;
        [SerializeField] float radius;
        [SerializeField] bool scaleFx = true;
        [Header("Damage")]
        [SerializeField] LayerMaskType allyLayer = LayerMaskType.HitBoxLayer;
        [SerializeField] DamageConfig damageConfig;
        [Header("Feedback")]
        [SerializeField] ParticleSystem healFxPrefab;
        [SerializeField] Feedback healFeedback;

        public float Radius => radius;

        HealAoeActionData _data;
        Vector3 _healPosition;
        IActor _actor;

        public override void Trigger(object target, IActor actor)
        {
            _data = Get<HealAoeActionData>(target);
            _healPosition = _data.Position;
            _actor = actor;
            HealAoe();
        }

        void HealAoe()
        {
            var realRadius = _data.IsOverrideRadius ? _data.OverrideRadius : Radius;

            if (healFxPrefab)
            {
                FxHelper.SpawnExplodeFx(pools, _healPosition, Vector3.zero, realRadius, scaleFx, healFxPrefab, healFeedback,
                    _actor);
            }

            var combinedDamage = CombinedDamageData.Combine(_data.DamageSourceData, damageConfig);
            ExplodeHelper.ScanHealAoe(_healPosition, realRadius, _actor.TeamConfig.GetLayerMask(allyLayer), combinedDamage.Damage, healType);
        }
    }
    
    public struct HealAoeActionData
    {
        public Vector3 Position;
        public float RadiusBonus;
        public DamageSourceData DamageSourceData;
        public bool IsOverrideRadius;
        public float OverrideRadius;
        
        public HealAoeActionData(Vector3 position, float radiusBonus, DamageSourceData damageSourceData, bool isOverrideRadius = false, float overrideRadius = -1)
        {
            Position = position;
            RadiusBonus = radiusBonus;
            DamageSourceData = damageSourceData;
            IsOverrideRadius = isOverrideRadius;
            OverrideRadius = overrideRadius;
        }
    }
}