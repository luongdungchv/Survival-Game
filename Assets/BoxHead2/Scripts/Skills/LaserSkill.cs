using System.Collections;
using System.Linq;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/LaserSkill")]
    public class LaserSkill : Skill
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
        
        [Header("Weapon")]
        [SerializeField] bool useCurrentWeapon;
        [SerializeField] AttachConfig[] attachConfigs;

        [SerializeField] Feedback prepareFeedback, loopFeedback;

        [Header("Damage")] 
        [SerializeField] DamageConfig damageConfig;
        [SerializeField] LayerMask enemyLayerMask;
        public override bool IsStopConditionMet => _stopped;
        bool _stopped;
        Vector3 _aimDir;
        bool _tracking, _shoot;
        Weapon[] _weapons;
        float[] _lastTimeDealDamage;

        LaserWeapon[] _laserWeapon;
        CombinedDamageData _combinedDamage;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _stopped = false;
            Actor.StopMovement();
            _tracking = false;
            if (useCurrentWeapon)
            {
                _weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
            }
            else
            {
                _weapons = SkillHelper.AttachWeapons(pools, Actor, null, attachConfigs, false);
            }
            
            _laserWeapon = _weapons.OfType<LaserWeapon>().ToArray();
            
            var damageSource = Actor as IDamageSource;
            _combinedDamage = CombinedDamageData.Combine(damageSource.GetDamageSourceData(), damageConfig);
            
            if (trackingSnapOnBegin)
            {
                Actor.RotateDirection(GetAimedDirection(), 0, immediately: true);
            }

            _lastTimeDealDamage = new float[_laserWeapon.Length];
            for (var i = 0; i < _lastTimeDealDamage.Length; i++)
            {
                _lastTimeDealDamage[i] = 0;
            }
        }

        protected override void DoStop()
        {
            base.DoStop();
            SkillHelper.StopWeapon(_weapons);
            if (!useCurrentWeapon)
            {
                SkillHelper.DetachWeapons(pools, _weapons, false);
            }
            _weapons = null;
        }
        
        protected override IEnumerator IEPerform()
        {
            
            Actor.PlayAnimation(animation, 0, animSpeed, animFade, false);
            Actor.SetRootMotionMult(rootMotionMult);

            foreach (var weapon in _weapons)
            {
                weapon.OnChargeFull();
            }
            while (true)
            {
                if (_tracking)
                {
                    _aimDir = GetAimedDirection();
                    Actor.RotateDirection(_aimDir, trackingSpeed);
                    if (showAimLine && _weapons != null)
                    {
                        foreach (var weapon in _weapons)
                        {
                            weapon.ShowIndicator(_aimDir, GetAimedPosition(), 40f, 1, 0);
                        }
                    }
                }

                if (_shoot && _laserWeapon != null)
                {
                    for (var index = 0; index < _laserWeapon.Length; index++)
                    {
                        var laser = _laserWeapon[index];
                        if (Physics.SphereCast(laser.LauncherPosition, Radius, laser.LauncherForward, out var hit,
                                Range, enemyLayerMask))
                        {
#if UNITY_EDITOR
                            Debug.DrawLine(laser.LauncherPosition, hit.point);
#endif
                            laser.UpdateLaserDistance(hit.distance, true, hit.point);
                            if (Time.time - _lastTimeDealDamage[index] > 0.2f)
                            {
                                var damageTaker = hit.collider.gameObject.GetComponentInParent<IDamageTaker>();
                                if (damageTaker is { Alive: true })
                                {
                                    _lastTimeDealDamage[index] = Time.time;
                                    _combinedDamage.UpdateDamageForce((damageTaker.Position - Actor.Position).normalized, hit.point, hit.point);
                                    damageTaker.TakeDamage(_combinedDamage);
                                }
                            }
                        }
                        else
                        {
                            laser.UpdateLaserDistance(Range, false, Vector3.zero);
                        }
                    }
                }
                yield return null;
            }
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            _tracking = true;
            _aimDir = GetAimedDirection();
        }

        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            _tracking = false;
            _aimDir = GetAimedDirection();
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            SkillHelper.PrepareShoot(_weapons);
            if (prepareFeedback)
            {
                prepareFeedback.Play();
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            _shoot = true;
            _tracking = false;
            loopFeedback?.Play();
            if (trackingSnapOnShoot)
            {
                _aimDir = GetAimedDirection();
                if (stickManSnapOnShoot)
                {
                    Actor.RotateDirection(_aimDir, 0, immediately: true);
                }
            }
            if (_aimDir == Vector3.zero)
            {
                _aimDir = GetAimedDirection();
            }
            Actor.DealingDamage = true;
            SkillHelper.Shoot(_weapons);
        }

        public override void OnEndAttack()
        {
            base.OnEndAttack();
            if (_laserWeapon is { Length: > 0 })
            {
                foreach (var laser in _laserWeapon)
                {
                    laser.OnStopUse();
                }
            }
            
            loopFeedback?.Stop();
            _shoot = false;
            _stopped = true;
        }
    }
}