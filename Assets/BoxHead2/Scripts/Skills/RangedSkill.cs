using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using BoxHead2.SubCombat;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/Ranged")]
    public class RangedSkill : Skill
    {
        [Header("Animation")]
        [SerializeField] protected string animation;
        [SerializeField] protected float animSpeed = 1;
        [SerializeField] protected bool animFade = true;
        [Header("Tracking")]
        [SerializeField] protected float trackingSpeed = 5;
        [SerializeField] bool trackingSnapOnBegin;
        [SerializeField] bool trackingSnapOnShoot;
        [SerializeField] bool stickManSnapOnShoot = true;
        [SerializeField] protected bool showAimLine;
        [Header("Shoot")]
        [FormerlySerializedAs("snapToTarget")]
        [SerializeField] bool snapDirectionToTarget;
        [SerializeField] bool snapDirectionToTargetMinYAngle;
        [SerializeField] bool clampRangeOnSnapDirection;
        [SerializeField] float randomRange;
        [SerializeField] bool shootForward;
        [Header("Shoot time")]
        [SerializeField] int shootTime = 1;
        [SerializeField] float shootTimeDelay = 0.1f;
        [SerializeField] float shootTimeSpread = 5f;
        [SerializeField] float shootTimeDamageBonus;
        [SerializeField] float shootTimeRangeBonus;
        [Header("Shoot spread")]
        [SerializeField] protected float spreadAngleStep;
        [SerializeField] float randomAngleStep;
        [SerializeField] protected int spreadCount = 1;
        [SerializeField] int randomSpreadCount;
        [SerializeField] float spreadDamageBonus;
        [Header("Weapon")]
        [SerializeField] bool useCurrentWeapon;
        [SerializeField] AttachConfig[] attachConfigs;
        [Header("Missile")]
        [SerializeField] protected bool randomMissile;
        [SerializeField] MissileData[] missileData;
        [Header("Feedback")]
        [SerializeField] float selfKnockBack;
        [SerializeField] Feedback prepareFeedback;
        [SerializeField] Feedback shootFeedback;
        [SerializeField] DisplayText shootTimeDisplayText;
        [SerializeField] DisplayText shootDisplayText;
        [Header("Indicator")]
        [SerializeField] SkillIndicator indicatorPrefab;
        [SerializeField] Vector3 launchPosOffset;
        [SerializeField] bool indicatorPerSpread;

        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        public float AnimSpeed => animSpeed;
        public virtual float SpreadAngleStep => spreadAngleStep;

        protected Weapon[] weapons;
        protected float knockBack;
        protected Vector3 knockBackDir;
        protected bool tracking;
        protected Vector3 aimDir;
        protected Vector3 aimPos;
        bool _isHit;
        int _totalMissile;
        int _cacheOnHitMissile;
        int _totalHit;
        
        SkillIndicator[] indicators;

        protected override void PrepareSkill()
        {
            Actor.StopMovement();
            tracking = false;
            aimDir = Vector3.zero;
            _isHit = false;
            _cacheOnHitMissile = 0;
            _totalMissile = 0;
            _totalHit = 0;
            if (useCurrentWeapon)
            {
                weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
            }
            else
            {
                weapons = SkillHelper.AttachWeapons(pools, Actor, null, attachConfigs, false);
            }
            if (trackingSnapOnBegin)
            {
                Actor.RotateDirection(GetAimedDirection(), 0, immediately: true);
            }
        }
        
        protected override void DoStop()
        {
            base.DoStop();
            SkillHelper.StopWeapon(weapons);
            if (!useCurrentWeapon)
            {
                SkillHelper.DetachWeapons(pools, weapons, false);
            }
            weapons = null;
            DespawnIndicator();
        }

        protected override IEnumerator IEPerform()
        {
            knockBack = 0;
            knockBackDir = Vector3.zero;
            Actor.PlayAnimation(animation, 0, AnimSpeed, animFade, false);
            Actor.SetRootMotionMult(rootMotionMult);

            while (true)
            {
                if (tracking)
                {
                    aimDir = GetAimedDirection();
                    aimPos = GetAimedPosition();
                    Actor.RotateDirection(aimDir, trackingSpeed);
                    if (showAimLine && weapons != null)
                    {
                        foreach (var weapon in weapons)
                        {
                            weapon.ShowIndicator(aimDir, aimPos, Range, spreadCount, spreadAngleStep);
                        }
                    }
                    UpdateIndicator(aimDir);
                }
                if (knockBack > 0)
                {
                    Actor.MoveDirection(knockBackDir, knockBack);
                    knockBack -= knockBack * 10 * Time.deltaTime;
                }
                yield return null;
            }
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            tracking = true;
            aimDir = GetAimedDirection();
            aimPos = GetAimedPosition();
        }

        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            tracking = false;
            aimDir = GetAimedDirection();
            aimPos = GetAimedPosition();
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            SkillHelper.PrepareShoot(weapons);
            if (prepareFeedback)
            {
                prepareFeedback.Play();
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            tracking = false;
            if (trackingSnapOnShoot)
            {
                aimDir = GetAimedDirection();
                aimPos = GetAimedPosition();
                if (stickManSnapOnShoot)
                {
                    Actor.RotateDirection(aimDir, 0, immediately: true);
                }
            }
            if (aimDir == Vector3.zero)
            {
                aimDir = GetAimedDirection();
                aimPos = GetAimedPosition();
            }

            if (shootForward)
            {
                aimDir = Actor.Transform.forward;
                aimPos = Actor.LockPosition + aimDir * Range;
            }
            Actor.DealingDamage = true;
            var aimedEnemy = GetAimedEnemy();
            SkillHelper.Shoot(weapons);
            var shootTimeCount = GetShootTime(Actor);
            var shootSpreadCount = GetSpread(Actor);
            SkillHelper.OnShoot(pools, new ShootData(Actor, GetDamageSourceData(), GetMissileData(index), weapons,
                Actor.Position, Actor.Position + Vector3.up, aimDir, shootForward,
                aimedEnemy, aimPos, shootTimeCount, shootTimeDelay, shootTimeSpread, shootTimeDamageBonus, shootTimeRangeBonus,
                SpreadAngleStep, randomAngleStep, shootSpreadCount, randomSpreadCount, spreadDamageBonus, Range * aimDir.magnitude, randomRange, 
                GetRadiusBonus(), snapDirectionToTarget, clampRangeOnSnapDirection, shootFeedback, AfterDespawnMissileAction, snapDirectionToTargetMinYAngle, OnMissileCompleted));

            _totalMissile += shootTimeCount * shootSpreadCount;

            knockBack = selfKnockBack;
            knockBackDir = -aimDir;
            
            if (shootDisplayText.ShouldDisplay)
            {
                shootDisplayText.Display(Actor);
            }

            DespawnIndicator();
        }

        void OnMissileCompleted(int hitCount)
        {
            if (_dealDamageCount == null) return;
            
            _totalHit += hitCount;
            _cacheOnHitMissile++;
            if (_cacheOnHitMissile == _totalMissile)
            {
                _dealDamageCount?.Invoke(_totalHit);
            }
        }

        public override void OnStopIndicator(int index)
        {
            base.OnStopIndicator(index);
            DespawnIndicator();
        }

        protected virtual void AfterDespawnMissileAction(MissileHitData missileHitData)
        {
            if (!_isHit)
            {
                _isHit = true;
                _isDealDamage?.Invoke(true);
            }
        }

        int GetShootTime(IActor actor)
        {
            return shootTime;
        }

        int GetSpread(IActor actor)
        {
            return spreadCount;
        }

        protected virtual float GetRadiusBonus()
        {
            return 0;
        }

        protected virtual MissileData GetMissileData(int index)
        {
            index = Mathf.Clamp(index, 0, missileData.Length - 1);
            return randomMissile ? missileData[Random.Range(0, missileData.Length)] : missileData[index];
        }
        
        protected virtual void UpdateIndicator(Vector3 dir)
        {
            if (!indicatorPrefab) return;
            
            if (indicators == null)
            {
                if (indicatorPerSpread)
                {
                    var spread = GetSpread(Actor);
                    indicators = new SkillIndicator[spread];
                    for (var i = 0; i < spread; i++)
                    {
                        indicators[i] = pools.Spawn(indicatorPrefab);
                    }
                }
                else
                {
                    indicators = new SkillIndicator[1];
                    indicators[0] = pools.Spawn(indicatorPrefab);
                }
            }
            if (indicators != null)
            {
                var spread = GetSpread(Actor); 
                var horizontalOffset = new Vector3(launchPosOffset.x, 0, 0);
                var pos = Actor.Transform.TransformPoint(horizontalOffset);
                var launchPos = Actor.Transform.TransformPoint(launchPosOffset);
                var rangeOffset = new Vector3(0, 0, launchPosOffset.z).magnitude;
                var targetPos = GetAimedPosition();
                var targetPosOffset = (Actor.AimedEnemy != null ? Vector3.zero : Actor.Transform.rotation * horizontalOffset);
                var step = SpreadAngleStep;
                if (indicatorPerSpread)
                {
                    var angle = -step * (spread - 1) * 0.5f + (spread % 2 == 0 ? step * 0.5f : 0);
                    for (var i = 0; i < spread; i++)
                    {
                        if (i == (spread - 1) / 2)
                        {
                            indicators[i].Set(pos, launchPos, targetPos + targetPosOffset, Quaternion.Euler(0, angle, 0) * dir, Range, Radius, base.Radius, 0, 0);
                        }
                        else
                        {
                            indicators[i].Set(launchPos, launchPos, targetPos, Quaternion.Euler(0, angle, 0) * dir, Range - rangeOffset, Radius, base.Radius, 0, 0);    
                        }
                        angle += step;
                    }
                }
                else
                {
                    indicators[0].Set(pos, launchPos, targetPos + targetPosOffset, dir, Range, Radius, base.Radius, step * (spread -1), SpreadAngleStep * (spread -1));  
                }
            }
        }

        public override void OnChargeFull(int index)
        {
            if (weapons != null)
            {
                foreach (var wp in weapons)
                {
                    wp.OnChargeFull();
                }
            }
        }

        protected void DespawnIndicator()
        {
            if (indicators != null)
            {
                for (var i = 0; i < indicators.Length; i++)
                {
                    pools.Despawn(indicators[i].gameObject);   
                }
                indicators = null;
            }
        }
    }

    public class ShootData
    {
        public IActor Actor;
        public DamageSourceData SourceData;
        public MissileData MissileData;

        public Weapon[] Weapons;
        public Vector3 Position; // damage source position
        public Vector3 LaunchPos; // shoot original position
        public Vector3 Direction; // shoot direction
        public bool IsShootForward;
        
        public IDamageTaker Target; // targeted damage taker
        public Vector3 TargetPos; // targeted position

        public int ShootTime; // num of continuous shoots
        public float ShootTimeDelay; // interval of continuous shoots
        public float ShootTimeSpread; // random spread of continuous shoots
        public float ShootTimeDamageReduce; // reduce damage of each continuous shoots
        public float ShootTimeRangeBonus;  // increase range of each continuous shoots
        
        public int SpreadCount; // num of concurrent shoots
        public int RandomSpreadCount; // random bonus num of concurrent shoots
        public float SpreadAngleStep; // angle between concurrent shoots
        public float RandomAngleStep; // random bonus angle between concurrent shoots
        public float SpreadDamageReduce; // reduce damage of each concurrent shoots
        
        public float Range; // range of each shoot
        public float RandomRange; // random bonus range (from -x to x) of each shoot
        public float RadiusBonus; // bonus radius of explosion
        public bool SnapDirectionToTarget; // snap direction to target
        public bool SnapDirectionToTargetMinYAngle; // snap direction to target
        public bool ClampRangeOnSnapDirection; // snap direction to target
        
        public Feedback ShootFeedback;
        public Action<MissileHitData> OnHitAction;
        public Action<int> TotalHitEnemy;

        public ShootData(IActor actor, DamageSourceData sourceData, MissileData missileData, Weapon[] weapons, 
            Vector3 position, Vector3 launchPos, Vector3 direction, bool isShootForward, IDamageTaker target, Vector3 targetPos, 
            int shootTime, float shootTimeDelay, float shootTimeSpread, float shootTimeDamageReduce, float shootTimeRangeBonus,
            float spreadAngleStep, float randomAngleStep, int spreadCount, int randomSpreadCount, float spreadDamageReduce,
            float range, float randomRange, float radiusBonus, bool snapDirectionToTarget, bool clampRangeOnSnapDirection, Feedback shootFeedback, Action<MissileHitData> onHitAction, bool snapDirectionToTargetMinYAngle, Action<int> totalHitEnemy)
        {
            Actor = actor;
            SourceData = sourceData;
            MissileData = missileData;
            Weapons = weapons;
            Position = position;
            LaunchPos = launchPos;
            Direction = direction;
            IsShootForward = isShootForward;
            Target = target;
            TargetPos = targetPos;
            ShootTime = shootTime;
            ShootTimeDelay = shootTimeDelay;
            ShootTimeSpread = shootTimeSpread;
            ShootTimeDamageReduce = shootTimeDamageReduce;
            ShootTimeRangeBonus = shootTimeRangeBonus;
            SpreadAngleStep = spreadAngleStep;
            RandomAngleStep = randomAngleStep;
            SpreadCount = spreadCount;
            RandomSpreadCount = randomSpreadCount;
            SpreadDamageReduce = spreadDamageReduce;
            Range = range;
            RandomRange = randomRange;
            RadiusBonus = radiusBonus;
            SnapDirectionToTarget = snapDirectionToTarget;
            ClampRangeOnSnapDirection = clampRangeOnSnapDirection;
            ShootFeedback = shootFeedback;
            OnHitAction = onHitAction;
            SnapDirectionToTargetMinYAngle = snapDirectionToTargetMinYAngle;
            TotalHitEnemy = totalHitEnemy;
        }
    }

    public class MissileHitData
    {
        public int MissileIndex;
        public Vector3 HitPos;
    }
}