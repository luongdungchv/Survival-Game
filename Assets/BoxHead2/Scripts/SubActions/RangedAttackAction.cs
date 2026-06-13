using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using BoxHead2.Skills;
using UnityEngine;
using UnityEngine.Serialization;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/RangedAttackAction")]
    public class RangedAttackAction : SubAction
    {
        [Header("Tracking")]
        [SerializeField] bool trackingSnapOnShoot;
        [SerializeField] bool randomTarget;
        [Header("Shoot")]
        [SerializeField] float range;
        [SerializeField] float randomRange;
        [SerializeField] int shootTime = 1;
        [SerializeField] float shootTimeDelay = 0.1f;
        [SerializeField] float shootTimeSpread = 5f;
        [SerializeField] float shootTimeDamageReduce = 0f;
        [SerializeField] float shootTimeRangeBonus = 0f;
        [SerializeField] float spreadAngleStep = 0;
        [SerializeField] float randomAngleStep = 0;
        [SerializeField] int spreadCount = 1;
        [SerializeField] int randomSpreadCount;
        [FormerlySerializedAs("snapToTarget")] [SerializeField] bool snapDirectionToTarget;
        [SerializeField] bool clampRangeOnSnapDirection;
        [Header("Weapon")] 
        [SerializeField] bool useCurrentWeapon = true;
        [SerializeField] AttachConfig[] attachConfigs;
        [Header("Missile")]
        [SerializeField] MissileData missileData;
        [Header("Feedback")]
        [SerializeField] Feedback shootFeedback;

        public override void Trigger(object target, IActor actor)
        {
            var data = Get<RangedAttackActionData>(target);
            var weapons = useCurrentWeapon ? SkillHelper.GetAttachedEquipments<Weapon>(data.Actor, attachConfigs) : null;
            var enemy = randomTarget ? data.Actor.GetRandomEnemy(range, new HashSet<IDamageTaker> {data.Actor.AimedEnemy}) : data.Actor.AimedEnemy;
            if (enemy == null) enemy = data.Actor.AimedEnemy;
            var dir = randomTarget && enemy != null? enemy.LockPosition - data.Actor.Position : data.Direction;
            dir.y = 0;
            if (dir == Vector3.zero)
            {
                dir = data.Actor.AimedDirection;
            }
            if (trackingSnapOnShoot)
            {
                data.Actor.RotateDirection(data.Actor.AimedDirection, 0, immediately: true);
            }
            SkillHelper.Shoot(weapons);
            SkillHelper.OnShoot(pools, new ShootData(data.Actor, data.SourceData, missileData, weapons,
                data.Actor.Position, data.Actor.Position + Vector3.up, dir.normalized, false, enemy, Vector3.zero,
                GetShootTime(actor), shootTimeDelay, shootTimeSpread, shootTimeDamageReduce, shootTimeRangeBonus,
                spreadAngleStep, randomAngleStep, spreadCount, randomSpreadCount,0, range, randomRange, 0, 
                snapDirectionToTarget, clampRangeOnSnapDirection, shootFeedback, null, false, null));
            if (displayText.ShouldDisplay)
            {
                displayText.Display(actor);
            }
        }

        int GetShootTime(IActor actor)
        {
            return shootTime;
        }
    }
    
    public struct RangedAttackActionData
    {
        public IActor Actor { get; }
        public DamageSourceData SourceData { get; }
        public Vector3 Direction { get; }

        public RangedAttackActionData(IActor actor, DamageSourceData sourceData, Vector3 direction)
        {
            Actor = actor;
            SourceData = sourceData;
            Direction = direction;
        }
    }
}