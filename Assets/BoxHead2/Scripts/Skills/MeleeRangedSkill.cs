using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Helper;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/MeleeRangedSkill")]
    public class MeleeRangedSkill : MeleeSkill
    {
        [SerializeField] MissileData missileData;
        [SerializeField] int shootTime;
        [SerializeField] float shootTimeDelay;
        [SerializeField] float shootTimeSpread;
        [SerializeField] float angleStep;
        [SerializeField] int spread;
        [SerializeField] float shootRange = 15f;

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            var aimDir = Actor.Transform.forward;
            var aimPos = Actor.LockPosition + aimDir * shootRange;

            Actor.DealingDamage = true;
            var aimedEnemy = GetAimedEnemy();
            SkillHelper.Shoot(Weapons);
            SkillHelper.OnShoot(pools, new ShootData(Actor, GetDamageSourceData(), GetMissileData(), Weapons,
                Actor.Position, Actor.Position + Vector3.up, aimDir, true,
                aimedEnemy, aimPos, shootTime, shootTimeDelay, shootTimeSpread, 0,
                0,
                angleStep, 0, spread, 0, 0,
                shootRange * aimDir.magnitude, 0,
                0, true, false, null,
                null, false, null));
        }

        MissileData GetMissileData()
        {
            return missileData;
        }
    }
}