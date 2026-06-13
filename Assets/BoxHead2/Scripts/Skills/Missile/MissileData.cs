using System;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using Dacodelaac.ObjectPooling;
using UnityEngine;

namespace BoxHead2.Skills
{
    [Serializable]
    public class MissileData
    {
        [SerializeField] Missile missilePrefab;
        [SerializeField] MissileFlyAction flyAction;
        [SerializeField] SubAction hitAction;
        [SerializeField] SubAction shootAction;
        [SerializeField] SubAction despawnAction;

        public Missile MissilePrefab => missilePrefab;

        public void CreateMissile(Pools pools, IActor actor, Vector3 source, Vector3 direction, IDamageTaker target,
            Vector3 targetPos, float range, float radiusBonus, DamageSourceData sourceData, Action<MissileHitData> onHitAction, int missileIndex, Action<int> totalHitEnemy)
        {
            var missile = pools.Spawn(missilePrefab);
            SetupMissile(missile, actor, source, direction, target, targetPos, range, radiusBonus, sourceData, onHitAction, missileIndex, totalHitEnemy);
        }

        public void SetupMissile(Missile missile, IActor actor, Vector3 source, Vector3 direction, IDamageTaker target,
            Vector3 targetPos, float range, float radiusBonus, DamageSourceData sourceData, Action<MissileHitData> onHitAction, int missileIndex, Action<int> totalHitEnemy)
        {
            missile.Setup(flyAction, hitAction, shootAction, despawnAction, 
                new MissileSetupData()
                {
                    Actor = actor,
                    Source = source, 
                    Direction = direction, 
                    Target = target,
                    TargetPos = targetPos,
                    Range = range,
                    RadiusBonus = radiusBonus
                }, sourceData, onHitAction, missileIndex, totalHitEnemy);
            missile.Shoot();
        }
    }
}