using System.Collections;
using System.Collections.Generic;
using BoxHead2.Items;
using BoxHead2.Skills;
using Dacodelaac.Core;
using Dacodelaac.DebugUtils;
using Dacodelaac.ObjectPooling;
using UnityEngine;

namespace BoxHead2.Helper
{
    public static class SkillHelper
    {
        public static T[] GetAttachedEquipments<T>(IEquipmentAttachable attachable, AttachConfig[] attachConfigs)
            where T : BaseMono
        {
            if (attachConfigs == null || attachConfigs.Length == 0)
            {
                return attachable.Transform.GetComponentsInChildren<T>();
            }

            var weapons = new List<T>();
            foreach (var attachConfig in attachConfigs)
            {
                var slot = attachable.FindAttachSlot(attachConfig.Slot);
                if (slot) weapons.AddRange(slot.GetComponentsInChildren<T>());
            }

            return weapons.ToArray();
        }

        public static void StopWeapon(Weapon[] weapons)
        {
            if (weapons != null)
            {
                foreach (var weapon in weapons)
                {
                    weapon.OnStopUse();
                }
            }
        }

        public static void ToggleTrail(Weapon[] weapons, bool on)
        {
            foreach (var weapon in weapons)
            {
                weapon.ToggleTrail(on);
            }
        }

        public static void PrepareShoot(Weapon[] weapons)
        {
            foreach (var weapon in weapons)
            {
                weapon.PrepareShoot();
            }
        }

        public static void Shoot(Weapon[] weapons)
        {
            if (weapons != null)
            {
                foreach (var weapon in weapons)
                {
                    weapon.Shoot();
                }
            }
        }

        public static void OnShoot(Pools pools, ShootData shootData)
        {
            if (shootData.Weapons == null || shootData.Weapons.Length == 0)
            {
                Shoot(pools, shootData);
            }
            else
            {
                foreach (var weapon in shootData.Weapons)
                {
                    shootData.LaunchPos = weapon.LauncherPosition;
                    Shoot(pools, shootData);
                }
            }
        }

        public static float CalculateShootRange(Vector3 launchPos, Vector3 stickManPos, Vector3 dir, float range)
        {
            if (dir == Vector3.zero)
            {
                dir = Vector3.forward;
            }

            var r = range;
            var xDiff = launchPos.x - stickManPos.x;
            var yDiff = launchPos.z - stickManPos.z;
            var a = dir.x * dir.x + dir.z * dir.z;
            var b = 2 * (dir.x * (launchPos.x - stickManPos.x) + dir.z * (launchPos.z - stickManPos.z));
            var c = xDiff * xDiff + yDiff * yDiff - range * range;
            var dist = b * b - 4 * a * c;
            if (dist >= 0)
            {
                r = (-b + Mathf.Sqrt(dist)) / (2 * a);
            }

            if (float.IsNaN(r))
            {
                r = range;
            }

            return r;
        }

        static void Shoot(Pools pools, ShootData shootData)
        {
            var snapRange = 1000f;
            if (shootData.SnapDirectionToTarget)
            {
                if (shootData.Target != null)
                {
                    var posTarget = shootData.Target.LockPosition;
                    if (shootData.SnapDirectionToTargetMinYAngle)
                    {
                        posTarget.y = Mathf.Max(posTarget.y, shootData.LaunchPos.y);
                    }
                    shootData.Direction = posTarget - shootData.LaunchPos;
                }
                else
                {
                    shootData.Direction = shootData.TargetPos - shootData.LaunchPos;
                    shootData.Direction.y = 0;
                }

                //shootData.Direction.y = 0;
                snapRange = shootData.Direction.magnitude;
                shootData.Direction.Normalize();
            }
            else
            {
                shootData.Direction = shootData.TargetPos - shootData.LaunchPos;
                if (shootData.IsShootForward)
                {
                    shootData.Direction.y = 0;
                }
                snapRange = shootData.Direction.magnitude;
                shootData.Direction.Normalize();
            }

            shootData.Range = CalculateShootRange(shootData.LaunchPos, shootData.Position, shootData.Direction,
                shootData.Range);

            if (shootData.SnapDirectionToTarget && shootData.ClampRangeOnSnapDirection)
            {
                shootData.Range = Mathf.Min(snapRange, shootData.Range);
            }

            shootData.Actor.StartCoroutine(IEShoot(pools, shootData));
        }

        static IEnumerator IEShoot(Pools pools, ShootData shootData)
        {
            var damageBonus = shootData.SourceData.DamageBonus;
            for (var i = 0; i < shootData.ShootTime; i++)
            {
                var spreadCount = shootData.SpreadCount +
                                  Random.Range(-shootData.RandomSpreadCount, shootData.RandomSpreadCount + 1);
                var angleStep = shootData.SpreadAngleStep +
                                Random.Range(-shootData.RandomAngleStep, shootData.RandomAngleStep);
                var angle = 0f;

                if (i > 0 && angleStep == 0 && shootData.ShootTimeSpread > 0)
                {
                    angle = Random.Range(-shootData.ShootTimeSpread, shootData.ShootTimeSpread);
                }

                shootData.SourceData.DamageBonus = damageBonus;
                if (i > 0)
                {
                    shootData.SourceData.DamageBonus += shootData.ShootTimeDamageReduce;
                }

                var damageBonus2 = shootData.SourceData.DamageBonus;
                var range = shootData.Range + i * shootData.ShootTimeRangeBonus;
                for (var j = 0; j < spreadCount; j++)
                {
                    var angleTemp = angle + (j % 2 + j / 2) * angleStep * (j % 2 == 0 ? 1 : -1);
                    shootData.SourceData.DamageBonus = damageBonus2;
                    var range2 = range + Random.Range(-shootData.RandomRange, shootData.RandomRange);
                    if (j != (spreadCount - 1) / 2)
                    {
                        shootData.SourceData.DamageBonus += shootData.SpreadDamageReduce;
                    }

                    var missileData = shootData.MissileData;
                    var direction = Quaternion.Euler(0, angleTemp, 0) * shootData.Direction;
                    var targetPos = shootData.TargetPos;
                    if (!shootData.SnapDirectionToTarget)
                    {
                        var launchPos = shootData.LaunchPos;
                        //launchPos.y = 0;
                        targetPos = launchPos + direction.normalized * range2;
                    }
                    missileData.CreateMissile(pools, shootData.Actor, shootData.LaunchPos, direction, shootData.Target,
                        targetPos, range2, shootData.RadiusBonus, shootData.SourceData, shootData.OnHitAction, i * spreadCount + j, shootData.TotalHitEnemy);
                }

                if (shootData.ShootFeedback)
                {
                    shootData.ShootFeedback.Play();
                }

                yield return new WaitForSeconds(shootData.ShootTimeDelay);
            }
        }

        public static Weapon[] AttachWeapons(Pools pools, IEquipmentAttachable equipmentAttachable,
            WeaponData weaponData, AttachConfig[] attachConfigs, bool onBodyWeapon)
        {
            var ws = new List<Weapon>();
            if (!onBodyWeapon)
            {
                foreach (var config in attachConfigs)
                {
                    var slot = equipmentAttachable.FindAttachSlot(config.Slot);
                    if (slot)
                    {
                        var weapon = pools.Spawn(config.WeaponPrefab);
                        weapon.Setup(weaponData);
                        weapon.OnAttach(equipmentAttachable, slot, false);

                        ws.Add(weapon);
                    }
                }
            }
            else
            {
                foreach (var config in attachConfigs)
                {
                    var slot = equipmentAttachable.FindAttachSlot(config.Slot);

                    var weapons = slot.GetComponentsInChildren<Weapon>();
                    foreach (var weapon in weapons)
                    {
                        weapon.Setup(weaponData);
                        weapon.OnAttach(equipmentAttachable, slot, true);
                        ws.Add(weapon);
                    }
                }
            }

            return ws.ToArray();
        }

        public static void DetachWeapons(Pools pools, Weapon[] weapons, bool onBodyWeapon)
        {
            foreach (var weapon in weapons)
            {
                weapon.OnDetach();
                if (!onBodyWeapon && weapon != null)
                {
                    pools.Despawn(weapon.gameObject);
                }
            }
        }
        

        public static GameObject AttachGameObjectToWeaponSlot(Pools pools, IEquipmentAttachable equipmentAttachable, AttachConfig attachConfig, GameObject goPrefab)
        {
            var slot = equipmentAttachable.FindAttachSlot(attachConfig.Slot);
            if (slot)
            {
                var weapon = pools.Spawn(goPrefab, slot.Transform);
                weapon.transform.localScale = Vector3.one;
                return weapon;
            }
            
            return null;
        } 
    }
}