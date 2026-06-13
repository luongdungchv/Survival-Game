using System;
using BoxHead2.Actor;
using BoxHead2.Combat;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/AreaOfEffectWallAction")]
    public class AreaOfEffectWallAction : SubAction
    {
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] AreaOfEffectData areaOfEffectData;
        [SerializeField] AreaOfEffectWall areaOfEffectWallPrefab;

        public override void Trigger(object target, IActor actor)
        {
            var data = Get<AreaOfEffectWallActionData>(target);
            var areaOfEffectWall = pools.Spawn(areaOfEffectWallPrefab);
            areaOfEffectWall.Setup(areaOfEffectData, data.SourceData, data.Transform, actor.TeamConfig.GetLayerMask(damageLayer), data.StopCondition, data.Range, data.Direction);
        }
    }
    
    public struct AreaOfEffectWallActionData
    {
        public Transform Transform { get; }
        public DamageSourceData SourceData { get; }
        public Func<bool> StopCondition { get; }
        public float Range { get; }
        public Vector3 Direction { get; }

        public AreaOfEffectWallActionData(Transform transform, DamageSourceData sourceData,
            Func<bool> stopCondition, float range, Vector3 direction)
        {
            Transform = transform;
            SourceData = sourceData;
            StopCondition = stopCondition;
            Range = range;
            Direction = direction;
        }
    }
}