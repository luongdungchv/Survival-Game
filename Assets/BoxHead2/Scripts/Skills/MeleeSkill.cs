using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using BoxHead2.SubActions;
using Dacodelaac.DebugUtils;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/MeleeSkill")]
    public class MeleeSkill : Skill, ISubActionDataProvider<ExplodeActionData>, ISubActionDataProvider<HealAoeActionData>, ISubActionDataProvider<AreaOfEffectActionData>, ISubActionDataProvider<HealActionData>
    {
        [Header("Animation")]
        [SerializeField] protected string animation;
        [SerializeField] AnimationClip clip;
        [SerializeField] protected float animSpeed = 1;
        [SerializeField] protected bool animFade = true;
        [Header("Moving")]
        [SerializeField] float moveTowardSpeed;
        [SerializeField] float deceleration = 5f;
        [SerializeField] float trackingSpeed = 5;
        [SerializeField] protected Vector3 rotateOffset;
        [Header("Weapon")] 
        [SerializeField] bool useCurrentWeapon;
        [SerializeField] AttachConfig[] attachConfigs;
        [Header("HurtBox")]
        [SerializeField] float extraDamageFrame;
        [SerializeField] float hurtBoxResetInterval = -1;
        [SerializeField] HurtBoxGroup[] hurtBoxGroupPrefabs;
        [Header("Damage")]
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] MeleeHitData[] meleeHitsData;
        [Header("Feedback")]
        [SerializeField] Feedback beginTrailFeedback;
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] HitFeedback hitFeedback;
        [SerializeField] ParticleSystem[] feedbackFxPrefab;
        [SerializeField] Feedback[] feedbacks;
        [SerializeField] Vector3[] feedbacksOffset;

        [Header("Dynamic Dash Attack")] 
        [SerializeField] bool dynamicDashAttack;
        [SerializeField, ShowIf("dynamicDashAttack")] float minDashAttackRange = 1f;
        [SerializeField, ShowIf("dynamicDashAttack")] float dashFrontOffset = -1f;
        [SerializeField, ShowIf("dynamicDashAttack")] float dashAttackRange = 6f;
        
        [Header("On Hit Action")]
        [SerializeField] SubAction[] onHitSubAction;

        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        public float AnimSpeed => animSpeed * (1 + (Actor?.MeleeAttackSpeedBonus ?? 0));
        public float TrackingSpeed => trackingSpeed;
        public float MoveTowardSpeed => Actor.IsPreview ? 0f : moveTowardSpeed;

        float _dashBeforeAttackSpeed;
        
        protected bool _tracking;
        Vector3 _cacheDirection;
        bool _moving;
        float _currentFrame;
        float _lastFrame;
        float _moveSpeed;
        float _deceleration;
        bool _isTriggerHit = false;
        Vector3 _hitPosition;
        HurtBoxGroup[] _hurtBoxGroups; 
        protected HurtBox[] _hurtBoxes;
        protected Weapon[] Weapons;
        Dictionary<int, ParticleSystem> _trails;
        Coroutine _useRoutine;
        MeleeHitData _meleeHitData;
        protected CombinedDamageData _combinedDamageData;
        float _lastTimeClearHurtBox;
        bool _shouldClearHurtBox;
        protected AttackIndicator _attackIndicator;
        protected Vector3 _rotateOffset;
        
        public override T CreateCopy<T>()
        {
            var copy = base.CreateCopy<MeleeSkill>();
            if (onHitSubAction is { Length: > 0 })
            {
                copy.onHitSubAction = new SubAction[onHitSubAction.Length];
                for (var i = 0; i < copy.onHitSubAction.Length; i++)
                {
                    copy.onHitSubAction[i] = onHitSubAction[i].CreateCopy<SubAction>();
                }
            }

            return copy as T;
        }

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            Actor.StopMovement();
            _moving = false;
            _tracking = false;
            _isTriggerHit = false;
            _currentFrame = _lastFrame = 0;
            
            _trails = new Dictionary<int, ParticleSystem>();
            if (useCurrentWeapon)
            {
                Weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
            }
            else
            {
                Weapons = SkillHelper.AttachWeapons(pools, Actor, null, attachConfigs, false);
            }
            HurtBoxHelper.SetupHurtBoxes(pools, Actor, hurtBoxGroupPrefabs, out _hurtBoxGroups, out _hurtBoxes);
            SetupCombineDamage(0);
            _rotateOffset = rotateOffset;
        }

        protected override void DoStop()
        {
            base.DoStop();
            var damageTakerCount = _hurtBoxGroups.Sum(h => h.DamageTakers.Count);
            _dealDamageCount?.Invoke(damageTakerCount);
            HurtBoxHelper.DespawnHurtBoxes(pools, ref _hurtBoxGroups);
            SkillHelper.StopWeapon(Weapons);
            StopAllTrail();
            DespawnIndicator();
            if (!useCurrentWeapon)
            {
                SkillHelper.DetachWeapons(pools, Weapons, false);
            }
            Weapons = null;
        }

        protected override IEnumerator IEPerform()
        {
#if UNITY_EDITOR
            Dacoder.DrawCircle(Actor.Position, Range, Color.red, 1f);
#endif

            Actor.PlayAnimation(animation, 0, AnimSpeed, animFade, false);
            Actor.SetRootMotionMult(rootMotionMult);

            if (dynamicDashAttack && Actor is IDamageTaker { IsCinch: false, IsStaggering: false })
            {
                var beginTracking =
                    clip.events.FirstOrDefault(e => e.functionName == "OnBeginTracking");
                var stopTracking =
                    clip.events.FirstOrDefault(e => e.functionName == "OnStopTracking");
                if (beginTracking != null && stopTracking != null)
                {
                    var dashDuration = (stopTracking.time - beginTracking.time) / AnimSpeed;
                    var aimedEnemy = GetAimedEnemy();
                    var rangeToDash = aimedEnemy != null ? Vector3.Distance(aimedEnemy.Position, Actor.Position) : 0f;
                    if (rangeToDash < minDashAttackRange) rangeToDash = 0f;
                    if (rangeToDash > dashAttackRange) rangeToDash = 0;
                    rangeToDash += dashFrontOffset;
                    rangeToDash = Mathf.Max(0f, rangeToDash);
                    
                    if (dashDuration > 0 && rangeToDash > 0)
                    {
                        dashDuration = Mathf.Max(0.1f, dashDuration);
                        _dashBeforeAttackSpeed = rangeToDash / dashDuration;
                        Actor.StartDash();
                    }
                    else
                    {
                        _dashBeforeAttackSpeed = 0;
                    }
                }
                else
                {
                    _dashBeforeAttackSpeed = 0;
                }
            }
            else
            {
                _dashBeforeAttackSpeed = 0;
            }

            yield return new WaitForEndOfFrame();

            while (true)
            {
                if (_tracking)
                {
                    Actor.RotateDirection(Quaternion.Euler(_rotateOffset) * GetAimedDirection(), TrackingSpeed);
                    if (dynamicDashAttack)
                    {
                        Actor.MoveDirection(Actor.Transform.forward, _dashBeforeAttackSpeed);
                    }
                    UpdateIndicator();
                }
                if (_moving && _moveSpeed > 0)
                {
                    Actor.MoveDirection(Actor.Transform.forward, _moveSpeed);
                    _moveSpeed -= _moveSpeed * _deceleration * Time.deltaTime;
                }
                
                _shouldClearHurtBox = hurtBoxResetInterval > 0 && Time.time - _lastTimeClearHurtBox > hurtBoxResetInterval;
                if (_shouldClearHurtBox)
                {
                    _lastTimeClearHurtBox = Time.time;
                }
                
                HurtBoxHelper.ExecuteFrameBaseHurtBox(Actor, Actor.TeamConfig.GetLayerMask(damageLayer), animation, clip, 
                    _hurtBoxes, _combinedDamageData, _shouldClearHurtBox, extraDamageFrame, ref _currentFrame, ref _lastFrame,
                    OnHit);

                yield return null;
            }
        }

        protected virtual void OnHit(Vector3 pos, Vector3 dir, HitType hitType, IDamageTaker damageTaker)
        {
            FxHelper.SpawnFx(pools, pos, dir, hitFxPrefab, null);
            hitFeedback.Play(hitType);
            if (!_isTriggerHit)
            {
                _isTriggerHit = true;
                _isDealDamage?.Invoke(true);
                _hitPosition = pos;
                if (onHitSubAction != null)
                {
                    foreach (var subAction in onHitSubAction)
                    {
                        subAction.Trigger(this, Actor);
                    }
                }
            }
        }

        public override void OnBeginHit(int index)
        {
            base.OnBeginHit(index);
            SetupCombineDamage(index);
            DespawnIndicator();
            if (_meleeHitData.AttackIndicatorData != null)
            {
                _attackIndicator = _meleeHitData.AttackIndicatorData.Spawn(pools);
                UpdateIndicator();
            }
        }

        public override void OnBeginMove(int index)
        {
            base.OnBeginMove(index);
            _moving = true;
            _moveSpeed = MoveTowardSpeed;
            _deceleration = deceleration;
            Actor.DealingDamage = true;
        }

        public override void OnStopMove(int index)
        {
            base.OnStopMove(index);
            _moving = false;
            Actor.StopMovement();
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            _tracking = true;
        }

        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            _tracking = false;
            if (dynamicDashAttack)
            {
                Actor.StopDash();
            }
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            var particle = GetTrailFx(index);
            index = Mathf.Clamp(index, 0, meleeHitsData.Length - 1);
            if (particle)
            {
                var trail = pools.Spawn(particle, Actor.Transform);
                WeaponVisualSetter.Bind(Actor, trail.gameObject);
                foreach (var fx in trail.GetComponentsInChildren<ParticleSystem>())
                {
                    var main = fx.main;
                    main.simulationSpeed = meleeHitsData[index].TrailSpeed;
                }
                var t1 = particle.transform;
                var t2 = trail.transform;
                t2.localPosition = t1.localPosition;
                t2.localRotation = t1.localRotation;
                trail.Play();
                _trails.TryAdd(index, trail);
            }
            if (meleeHitsData[index].SpecificWeaponSlots != null && meleeHitsData[index].SpecificWeaponSlots.Length > 0)
            {
                var specificWeapon = SkillHelper.GetAttachedEquipments<Weapon>(Actor, meleeHitsData[index].SpecificWeaponSlots);
                SkillHelper.ToggleTrail(specificWeapon, true);
            }
            else
            {
                SkillHelper.ToggleTrail(Weapons, true);
            }
            if (beginTrailFeedback)
            {
                beginTrailFeedback.Play();
            }
        }

        protected virtual ParticleSystem GetTrailFx(int index)
        {
            var i = Mathf.Clamp(index, 0, meleeHitsData.Length - 1);
            return meleeHitsData[i].TrailFxPrefab;
        }

        void StopAllTrail()
        {
            foreach (var trail in _trails.Values)
            {
                if (!pools.Contains(trail.gameObject))
                {
                    trail.transform.parent = null;
                    if (trail.isPlaying)
                    {
                        trail.Stop();
                    }   
                }
            }
            _trails.Clear();
            SkillHelper.ToggleTrail(Weapons, false);
        }

        public override void OnStopTrail(int index)
        {
            base.OnStopTrail(index);
            if (_trails.ContainsKey(index))
            {
                if (_trails[index])
                {
                    if (!pools.Contains(_trails[index].gameObject))
                    {
                        _trails[index].transform.SetParent(null);
                        if (_trails[index].isPlaying)
                        {
                            _trails[index].Stop();
                        }   
                    }
                }
                _trails.Remove(index);
            }
            else
            {
                SkillHelper.ToggleTrail(Weapons, false);
            }
        }

        public override void OnFeedbackEvent(int index)
        {
            base.OnFeedbackEvent(index);
            var feedback = feedbacks.Length > index ? feedbacks[index] : null;
            var feedbackFx = feedbackFxPrefab.Length > index ? feedbackFxPrefab[index] : null;
            if (feedbackFx)
            {
                var fxOffset = feedbacksOffset.Length > index ? feedbacksOffset[index] : Vector3.zero;
                FxHelper.SpawnFx(pools, Actor.Transform.TransformPoint(feedbackFx.transform.position + fxOffset),
                    Actor.Transform.TransformDirection(feedbackFx.transform.forward),
                    feedbackFx, feedback);
            }
            else if (feedback)
            {
                feedback.Play();   
            }
        }

        protected virtual void UpdateIndicator()
        {
            if (_attackIndicator)
            {
                _attackIndicator.DoUpdate(Actor.Transform, Actor.Position + Actor.ForwardDirection * Range, Actor.ForwardDirection, Range, Radius);
            }
        }
        
        public override void OnStopIndicator(int index)
        {
            DespawnIndicator();
        }

        void DespawnIndicator()
        {
            if (_attackIndicator)
            {
                pools.Despawn(_attackIndicator.gameObject);
                _attackIndicator = null;
            }
        }

        protected virtual void SetupCombineDamage(int index)
        {
            if (meleeHitsData.Length == 0)
            {
                return;
            }
            index = Mathf.Clamp(index, 0, meleeHitsData.Length - 1);
            _meleeHitData = meleeHitsData[index];
            _combinedDamageData = CombinedDamageData.Combine(GetDamageSourceData(), _meleeHitData.DamageConfig);
        }
        
        ExplodeActionData ISubActionDataProvider<ExplodeActionData>.Get()
        {
            return new ExplodeActionData(_hitPosition, _hitPosition, 0, GetDamageSourceData(), new DamageConfig(), false);
        }

        HealAoeActionData ISubActionDataProvider<HealAoeActionData>.Get()
        {
            return new HealAoeActionData(_hitPosition, 0, GetDamageSourceData());
        }
        
        AreaOfEffectActionData ISubActionDataProvider<AreaOfEffectActionData>.Get()
        {
            return new AreaOfEffectActionData(null, _hitPosition, GetDamageSourceData(), 0);
        }
        
        HealActionData ISubActionDataProvider<HealActionData>.Get()
        {
            return new HealActionData(Actor);
        }

#if UNITY_EDITOR
        [ContextMenu("AutoSetTrailSpeed1")]
        public void AutoSetTrailSpeed1()
        {
            AutoSetTrailSpeed(0.1f);
        }
        
        [ContextMenu("AutoSetTrailSpeed3")]
        public void AutoSetTrailSpeed2()
        {
            AutoSetTrailSpeed(0.3f);
        }

        [Button]
        public void AttackDuration()
        {
            var ev =
                clip.events.FirstOrDefault(e => e.functionName == "OnCanMoveNextSkill");
            var time = ev.time / animSpeed;
            Debug.Log(time);
        }

        void AutoSetTrailSpeed(float originDuration)
        {
            for (var i = 0; i < meleeHitsData.Length; i++)
            {
                var meleeHitDaa = meleeHitsData[i];
                var beginTrailEvent =
                    clip.events.FirstOrDefault(e => e.functionName == "OnBeginTrail" && e.intParameter == i);
                var stopTrailEvent =
                    clip.events.FirstOrDefault(e => e.functionName == "OnStopTrail" && e.intParameter == i);
                if (beginTrailEvent != null && stopTrailEvent != null)
                {
                    var trailTargetDuration = (stopTrailEvent.time - beginTrailEvent.time) / animSpeed;
                    if (trailTargetDuration > 0)
                    {
                        meleeHitDaa.SetTrailSpeed(originDuration / trailTargetDuration);
                    }
                }

                meleeHitsData[i] = meleeHitDaa;
            }

            EditorUtility.SetDirty(this);
        }
#endif
        
    }

    [Serializable]
    public struct MeleeHitData
    {
        [FormerlySerializedAs("damageData")] [SerializeField] DamageConfig damageConfig;
        [SerializeField] ParticleSystem trailFxPrefab;
        [SerializeField] float trailSpeed;
        [SerializeField] AttachConfig[] specificWeaponSlots;
        [SerializeField] AttackIndicatorData attackIndicatorData;

        public DamageConfig DamageConfig => damageConfig;
        public ParticleSystem TrailFxPrefab => trailFxPrefab;
        public float TrailSpeed => trailSpeed;
        public AttachConfig[] SpecificWeaponSlots => specificWeaponSlots;
        public AttackIndicatorData AttackIndicatorData => attackIndicatorData;

#if UNITY_EDITOR
        public void SetTrailSpeed(float trailSpeed)
        {
            this.trailSpeed = trailSpeed;
        }
#endif
    }
}