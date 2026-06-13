using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoxHead2.AnimatorEventCustom;
using BoxHead2.Collection;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using BoxHead2.Skills;
using BoxHead2.States;
using BoxHead2.SubActions;
using BoxHead2.SubCombat;
using Dacodelaac.Core;
using Dacodelaac.FiniteStateMachine;
using Dacodelaac.Utils;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoxHead2.Actor
{
    public abstract class Actor : BaseMono, IActor, IDamageTaker, IDamageSource
    {
        [SerializeField] protected TextSpawner textSpawner;
        [SerializeField] protected Renderer[] renderers;
        [SerializeField] GameObject[] visualGo;
        [SerializeField] BuffFx[] passiveBuffFx;
        [SerializeField] ParticleSystem damageFxMain;
        [SerializeField] ParticleSystem deadFxMain;
        [SerializeField] ParticleSystem deadFxGround;
        [SerializeField] ParticleSystem slowFx;
        [SerializeField] ParticleSystem stunFx;
        [SerializeField] ParticleSystem warningFx;
        [SerializeField] ParticleSystem healFoodFx;
        [SerializeField] ParticleSystem dissolveFx;
        [SerializeField] ParticleSystem stealthFx;
        [SerializeField] ParticleSystem runTraceFX;
        [SerializeField] ParticleSystem trailFx;
        [SerializeField] ParticleSystem criticalFx;
        [SerializeField] ParticleSystem dashFx;
        [SerializeField] bool fxPosAtLock;
        [Header("Feedback")] [SerializeField] protected ActorFeedback feedback;

        public event Action<float, float> OnHpChanged;
        public event Action<IActor> OnDespawnEvent;
        public event Action OnStatusEffectChanged;
        public event Action<StatusEffectType, int> OnStatusEffectDmgTaken;
        public bool IsPreview { get; set; }
        public bool Destroyed => this == null || !this;
        public Vector3 Position => Transform.position;
        public float Radius => ActorMovement.Radius;
        public ActorType Type => Config.type;
        public HitType HitType => Config.hitType;

        public virtual bool CanBeTarget => Alive && !IsInvisible;
        public Vector3 LockPosition => LockPivot?.Transform.position ?? Position;
        public Vector3 HeadPosition => HeadPivot?.Transform.position ?? LockPosition;
        public Vector3 FXPosition => fxPosAtLock ? LockPosition : Position;
        public WeaponData Weapon { get; set; }
        public WeaponData RangeWeapon { get; set; }
        public Skill CurrentSkill { get; set; }
        public ActorConfig Config { get; set; }
        public bool IsDying { get; set; }
        public bool CanShowWarning { get; set; }
#if !DACODER_RELEASE || UNITY_EDITOR
        public bool IsInvincible { get; set; }
#else
        public bool IsInvincible => false;
#endif

        protected LockPivot _lockPivot;
        LockPivot LockPivot => gameObject.GetAndCacheComponentInChildren(ref _lockPivot);

        protected HeadPivot _headPivot;
        HeadPivot HeadPivot => gameObject.GetAndCacheComponentInChildren(ref _headPivot);
        ApplyMeshRendererParticle[] _particlesMesh;
        public ApplyMeshRendererParticle[] ParticlesMesh => gameObject.GetAndCacheComponentsInChildren(ref _particlesMesh);
        IActorMovement _actorMovement;
        public IActorMovement ActorMovement => gameObject.GetAndCacheComponentInChildren(ref _actorMovement);
        protected AnimatorEventListener _animatorEventListener;

        AnimatorEventListener AnimatorEventListener =>
            gameObject.GetAndCacheComponentInChildren(ref _animatorEventListener);

        protected BodyPart[] _bodyParts;
        BodyPart[] BodyParts => gameObject.GetAndCacheComponentsInChildren(ref _bodyParts);
        protected AllyHitBox[] _allyHitBoxes;
        public AllyHitBox[] AllyHitBoxes => gameObject.GetAndCacheComponentsInChildren(ref _allyHitBoxes);
        WeaponAttachSlot[] _attachSlots;
        Dictionary<WeaponAttachSlotType, WeaponAttachSlot> _attachSlotDict;
        public virtual bool CanPerformAction => !IsStaggering && !IsPulling && !IsKnockingDown 
                                                && (!Config.canGetHitAnim || !(StateMachine.CurrentState is GetHitBaseState));
        public bool IsCanMoveNextSkill { get; private set; }
        public bool CanSetMoveNextSkill { get; protected set; }

        public float Hp
        {
            get => _hp;
            protected set
            {
                if (IsInvincible)
                {
                    _hp = MaxHp;
                    TriggerHpChanged(MaxHp, value);
                }
                else if (_hp != value)
                {
                    var oldValue = _hp;
                    _hp = value;
                    TriggerHpChanged(oldValue, Hp);
                }
            }
        }

        public float HpRatio => Hp / MaxHp;
        
        public virtual float OffHandWeaponCommittedBonus => 0;
        public virtual float MaxHp => 0;

        public virtual float WeaponCriticalDamage => 0;
        public bool Attacking { get; set; }
        public bool Initialized { get; set; }
        public bool DashConstant { get; set; }
        public virtual bool Alive => HpRatio > 0;
        public virtual int WeaponDamage => 0;
        public virtual float WeaponCriticalChance => 0;
        public virtual float WeaponCommittedBonus => 0;
        public virtual int OffHandWeaponDamage => 0;
        public virtual float OffHandWeaponCriticalChance => 0;
        public virtual float OffHandWeaponCriticalDamage => 0;
        public virtual float DamageReflection => 0;
        public virtual float DamageReflectionByMaxHp => 0;
        public virtual bool DamageReflectionBuff => false;
        public virtual float CriticalChanceBonus => 0;
        public virtual float CriticalDamageBonus => 0;
        public virtual float MissChance => 0;
        public virtual float DamageBonus => 0;
        public virtual float FinalDamageBonus => 0;
        public virtual float FinalWeaponDamageBonus => 0;
        public virtual float WeaponDamageBonus => 0;
        public virtual float SlowDurationBonus => 0;
        public virtual float StaggerDurationBonus => 0;
        public virtual float BurnDurationBonus => 0;
        public virtual float PoisonDurationBonus => 0;
        public virtual float BleedDurationBonus => 0;
        public virtual float BurnDurationBonusFlat => 0;
        public virtual float PoisonDurationBonusFlat => 0;
        public virtual float PoisonDamageBonus => 0;
        public virtual float BleedDamageBonus => 0;
        public virtual float BleedDurationBonusFlat => 0;
        public virtual int BleedStackBonus => 0;
        public virtual int BurnStackBonus => 0;
        public virtual float BurnDamageBonus => 0;
        public virtual bool DealBurnSpread => false;
        public virtual int PoisonStackBonus => 0;
        public virtual int DotStackBonus => 0;
        public virtual float DamageDotReduction => 0;
        public virtual float BonusDamageDot => 0;
        public virtual float PetDamageBonus => 0;
        public virtual float FragmentSoulsBonusChance => 0;
        public virtual float LuckOfTheSea => 0;
        public virtual float MoveSpeedBonus => 0;
        public virtual float LootingBonus => 0;
        public virtual float MeleeAttackSpeedBonus => 0;
        public virtual float FinalDamageTakenBonus => 0;
        public virtual bool IsPenetrateRanged => false;
        public virtual bool StaggerResist => false;
        public virtual bool IgnoreDamage => false;
        public virtual float StaggerChanceBonus => 0;
        public virtual Vector3 AimedDirection => Transform.forward;
        public float MovementCollisionRadius => ActorMovement.Radius;
        public IDamageTaker AimedEnemy { get; protected set; }
        public virtual Vector3 ForwardDirection => Transform.forward;
        public DamageTakerCollection AllyCollection => TeamConfig.allyCollection;
        public DamageTakerCollection EnemyCollection => TeamConfig.enemyCollection;
        public virtual bool IsPlayer => false;
        public bool IsPulling => DamageForce is { Type: DamageForceType.Pull, Duration: > 0f };
        public bool IsKnockingBack => DamageForce is { Type: DamageForceType.KnockBack, Duration: > 0f };

        public bool IsKnockingDown => DamageForce is { Type: DamageForceType.Knockdown, Duration: > 0f };
        public float AnimatorSpeed => _animatorSpeed * (!IsPlayingAnim ? SlowFactor : 1f);
        public virtual bool IgnoreDamageForce => CurrentSkill && CurrentSkill.IgnoreDamageForce;
        public bool CanBePulled => Config.canBePulled && !IgnoreDamageForce && _pullHp > 0;
        public bool CanBeKnockBack => !IgnoreDamageForce && !IsSkillMoving && !IsIgnoreKnockBack && !IsKnockingDown && Config.canBeKnockBack;
        public virtual bool CanBeKnockDown => !IgnoreDamageForce && !KnockdownResist && Config.canBeKnockDown;
        public virtual bool CanGetHitAnim => !IsStaggering && !IsKnockingDown && Config.canGetHitAnim;
        public virtual bool KnockdownResist => false;
        public bool IsStaggering => _stagger != null;
        public bool IsCinch => _cinch != null;
        public bool IsBlind => _blind != null;
        public bool IsSlowing => _slow is { Count: > 0 };
        public bool IsBurning => _burn is { Count: > 0 };
        public bool IsPoisoning => _poison is { Count: > 0 };
        public bool IsBleeding => _bleed is { Count: > 0 };
        public virtual float DamageReduction => 0;
        public virtual float Evade => 0;
        public virtual float DamageTaken => 0;
        public virtual float LifeSteal => 0;
        public virtual float HealExtra => 0;
        public virtual float LifeStealByEnemyHpDie => 0;
        public virtual float HealRefreshmentBonus => 0;
        public bool IsPlayingAnim { get; private set; }
        public bool IsInvisible { get; set; }
        public virtual float RunSpeed => 0;
        public virtual float RotateSpeed => Config.rotateSpeed * SlowFactor;
        public Animator Animator => this.gameObject.GetAndCacheComponentInChildren(ref this._animator); 
        public TeamConfig TeamConfig { get; set; }
        public bool DealingDamage { get; set; }
        public event Action OnBuffChangedEvent;
        public int RangedPerfectCount { get; set; }
        public float LastTimeRangedPerfect { get; set; }
        public bool IsSkillMoving { get; set; }
        public float LastTimeAttack { get; set; }
        public float LastTimeDealDamage { get; set; }
        public float LastTimeGetHit { get; set; }
        public float LastTimeTakeDamage { get; set; }
        public int GetHitCount { get; set; }
        public bool IsIgnoreKnockBack { get; set; }
        public CombinedDamageForceData DamageForce { get; set; }
        public bool CanBeSlow => Config.canBeSlow;
        public bool CanBeStagger => Config.canBeStagger;
        public bool CanBeCinch => Config.canBeCinch;
        public bool IsBountyMark => Time.time < _bountyMarkExpiredTime;
        public bool CanBeMark => Time.time >= _markCoolDownTime;
        public bool IsIdle => StateMachine.CurrentState is IIdleState;
        public int FinalMeleeWeaponDamage => Mathf.RoundToInt(WeaponDamage * (1f + WeaponDamageBonus + DamageBonus) * (1f + FinalWeaponDamageBonus + FinalDamageBonus));
        
        protected IDamageTaker AimedTarget;
        protected IDamageTaker NearestTarget;

        protected float LastTimeFreezeOrStaggerOrFear;
        public virtual bool IsMoving => false;
        List<BuffData> _buffs;
        protected Animator _animator;
        float _hp;

        public Dacodelaac.FiniteStateMachine.StateMachine StateMachine;
        float _rootMotionMult;
        float _nextTimePlayIdleFeedback;
        float _lastTimeHitFx;
        float _lastTimeSpawnDamageText;
        float _bountyMarkExpiredTime;
        float _markCoolDownTime;
        protected float _pullHp;
        const float MaxPullHp = 1;
        float _lastTimePull;
        protected bool RaisedDieEvent;
        bool _isLockVisual;
        float _animatorSpeed;
        public StatusEffect LastStatusEffect;
        protected float BleedIncreaseTakenDamage;
        protected float BurnIncreaseTakenDamage;
        protected float PoisonDeBuffDamage;

        protected float SlowFactor;


        int verticalHash, horizontalHash;

        // float _freezeFactor;
        // StatusEffect _freeze;
        StatusEffect _stagger;
        StatusEffect _cinch;
        StatusEffect _blind;
        List<StatusEffect> _slow;
        List<StatusEffect> _burn;
        List<StatusEffect> _bleed;
        List<StatusEffect> _poison;
        
        public List<StatusEffect> Burn => _burn;
        public List<StatusEffect> Poison => _poison;
        public List<StatusEffect> Bleed => _bleed;

        public override void UnbindVariable()
        {
            base.UnbindVariable();
            RemoveFromAllyCollection();
        }

        public override void ListenEvents()
        {
            base.ListenEvents();
            AnimatorListenEvent();
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            StopAnimatorListenEvent();
        }

        public void AnimatorListenEvent()
        {
            if (AnimatorEventListener)
            {
                AnimatorEventListener.OnBeginHitEvent += OnSkillBeginHitEvent;
                AnimatorEventListener.OnStopIndicatorEvent += OnSkillStopIndicatorEvent;
                AnimatorEventListener.OnBeginTrailEvent += OnSkillBeginTrail;
                AnimatorEventListener.OnStopTrailEvent += OnSkillStopTrail;
                AnimatorEventListener.OnBeginMoveEvent += OnSkillBeginMove;
                AnimatorEventListener.OnStopMoveEvent += OnSkillStopMove;
                AnimatorEventListener.OnBeginTrackingEvent += OnSkillBeginTracking;
                AnimatorEventListener.OnStopTrackingEvent += OnSkillStopTracking;
                AnimatorEventListener.OnBeginAttackEvent += OnSkillBeginAttack;
                AnimatorEventListener.OnEndAttackEvent += OnSkillEndAttack;
                AnimatorEventListener.OnCanMoveNextSkillEvent += OnSkillCanMoveNextSkill;
                AnimatorEventListener.OnEnterLocomotionStateEvent += OnEnterLocomotionState;
                AnimatorEventListener.OnAnimatorMoveEvent += OnAnimMove;
                AnimatorEventListener.OnPrepareShootEvent += OnSkillPrepareShoot;
                AnimatorEventListener.OnChargeFullEvent += OnSkillChargeFull;
                AnimatorEventListener.OnShootEvent += OnSkillShoot;
                AnimatorEventListener.OnCustomEventEvent += OnSkillCustomEvent;
                AnimatorEventListener.OnCustomEventEvent += OnStateCustomEvent;
                AnimatorEventListener.OnFeedbackEvent += OnSkillFeedbackEvent;
                AnimatorEventListener.OnFootEvent += OnFoot;
                AnimatorEventListener.OnJumpLandingEvent += OnJumpLanding;
                AnimatorEventListener.OnShowWarningEvent += OnShowWarning;
            }
        }

        protected virtual void OnShowWarning()
        {
            if (warningFx)
            {
                warningFx.Clear();
                warningFx.Play();
            }
        }

        public void StopAnimatorListenEvent()
        {
            if (AnimatorEventListener)
            {
                AnimatorEventListener.OnBeginHitEvent -= OnSkillBeginHitEvent;
                AnimatorEventListener.OnStopIndicatorEvent -= OnSkillStopIndicatorEvent;
                AnimatorEventListener.OnBeginTrailEvent -= OnSkillBeginTrail;
                AnimatorEventListener.OnStopTrailEvent -= OnSkillStopTrail;
                AnimatorEventListener.OnBeginMoveEvent -= OnSkillBeginMove;
                AnimatorEventListener.OnStopMoveEvent -= OnSkillStopMove;
                AnimatorEventListener.OnBeginTrackingEvent -= OnSkillBeginTracking;
                AnimatorEventListener.OnStopTrackingEvent -= OnSkillStopTracking;
                AnimatorEventListener.OnBeginAttackEvent -= OnSkillBeginAttack;
                AnimatorEventListener.OnEndAttackEvent -= OnSkillEndAttack;
                AnimatorEventListener.OnCanMoveNextSkillEvent -= OnSkillCanMoveNextSkill;
                AnimatorEventListener.OnEnterLocomotionStateEvent -= OnEnterLocomotionState;
                AnimatorEventListener.OnAnimatorMoveEvent -= OnAnimMove;
                AnimatorEventListener.OnPrepareShootEvent -= OnSkillPrepareShoot;
                AnimatorEventListener.OnChargeFullEvent -= OnSkillChargeFull;
                AnimatorEventListener.OnShootEvent -= OnSkillShoot;
                AnimatorEventListener.OnCustomEventEvent -= OnSkillCustomEvent;
                AnimatorEventListener.OnCustomEventEvent -= OnStateCustomEvent;
                AnimatorEventListener.OnFeedbackEvent -= OnSkillFeedbackEvent;
                AnimatorEventListener.OnFootEvent -= OnFoot;
                AnimatorEventListener.OnJumpLandingEvent -= OnJumpLanding;
                AnimatorEventListener.OnShowWarningEvent -= OnShowWarning;
            }
        }

        public override void DoEnable()
        {
            base.DoEnable();
            Initialized = false;
        }

        public override void Initialize()
        {
            base.Initialize();
            CanSetMoveNextSkill = true;
            InitProperties();
            InitSkills();
            InitStateMachine();
            Initialized = true;
            verticalHash = Animator.StringToHash("Vertical");
            horizontalHash = Animator.StringToHash("Horizontal");
        }

        public override void Tick()
        {
            if (!Initialized) return;
            StateMachine.Tick();
            if (IsDying) return;
            UpdateDirection();
            base.Tick();
            UpdateStatus();
            UpdateIdleFeedback();
            if (textSpawner)
            {
                textSpawner.Tick();
            }
        }

        void UpdateIdleFeedback()
        {
            if (!Alive || IsStaggering || IsPulling || IsKnockingDown || IsKnockingBack || Attacking) return;

            if (Time.time > _nextTimePlayIdleFeedback)
            {
                _nextTimePlayIdleFeedback = Time.time + Random.Range(5f, 10f);
                if (feedback.idleFeedback)
                {
                    feedback.idleFeedback.Play();
                }
            }
        }

        protected virtual void UpdateDirection()
        {
        }

        protected void UpdateStatus()
        {
            if (!Alive) return;
            UpdateStatusEffect();
        }

        protected virtual void UpdateStatusEffect()
        {
            UpdateDamageForce();
            UpdateStagger();
            UpdateSlow();
            UpdateCinch();
            UpdateBleed();
            UpdateBurn();
            UpdatePoison();
            UpdateBlind();
            UpdateAnimatorSpeed();
            //UpdateWarningFx();
        }
        
        void UpdateWarningFx()
        {
            // if (warningFx)
            // {
            //     warningFx.transform.position = HeadPosition;
            // }

            if (CanShowWarning)
            {
                if (warningFx && warningFx.isStopped && !IsInvisible) warningFx.Play();
            }
            else
            {
                if (warningFx && warningFx.isPlaying) warningFx.Stop();
            }
        }

        void UpdateDamageForce()
        {
            if (IsPulling)
            {
                _lastTimePull = Time.time;
                var dir = DamageForce.CenterPosition - Position;
                var force = dir.magnitude * DamageForce.Force;
                if (force > 0)
                {
                    var forceVector = EvaluatePullForce((DamageForce.Reverse ? -1 : 1) * force * dir.normalized,
                        DamageForce.Force, DamageForce.CanPullEscape);
                    ActorMovement.MoveDirection(forceVector.normalized, forceVector.magnitude);
                }
            }
            else if (IsKnockingDown || IsKnockingBack)
            {
                if (DamageForce.Force > 0)
                {
                    ActorMovement.Rotate(Transform.forward, Config.rotateSpeed);
                    ActorMovement.MoveDirection(DamageForce.HitDirection,
                        DamageForce.Force * (DamageForce.Reverse ? -1 : 1));
                    var damageForce = DamageForce;
                    damageForce.Force -= DamageForce.Force * Config.knockBackDeceleration * Time.deltaTime;
                    DamageForce = damageForce;
                }
            }

            if (DamageForce.Duration > 0)
            {
                var damageForce = DamageForce;
                damageForce.Duration -= Time.deltaTime;
                DamageForce = damageForce;
                if (DamageForce.Duration <= 0)
                {
                    ActorMovement.Stop();
                    ResetKnockBack();
                }
            }

            if (!IsPulling && Time.time - _lastTimePull > 1)
            {
                _pullHp = MaxPullHp;
            }
        }

        void UpdateBlind()
        {
            if (IsBlind)
            {
                _blind.Tick(this, Time.deltaTime);
            }
            
            if (IsBlind)
            {
                if (_blind.IsExpired)
                {
                    _blind = null;
                    OnRemoveStatusEffect();
                }
            }
        }

        void UpdateCinch()
        {
            if (IsCinch)
            {
                _cinch.Tick(this, Time.deltaTime);
            }
            
            if (IsCinch)
            {
                if (_cinch.IsExpired)
                {
                    _cinch = null;
                    OnRemoveStatusEffect();
                    EnableMovement();
                }
            }
        }

        void UpdateStagger()
        {
            if (IsStaggering)
            {
                _stagger.Tick(this, Time.deltaTime);
            }

            if (IsStaggering)
            {
                if (_stagger.IsExpired)
                {
                    _stagger = null;
                    OnRemoveStatusEffect();
                }
            }

            if (stunFx)
            {
                stunFx.transform.position = HeadPosition + Vector3.up * 0.5f;
            }

            if (IsStaggering)
            {
                if (stunFx && stunFx.isStopped) stunFx.Play();
            }
            else
            {
                if (stunFx && stunFx.isPlaying) stunFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        void UpdateSlow()
        {
            if (IsSlowing)
            {
                for (var index = _slow.Count - 1; index >= 0; index--)
                {
                    var slowEffect = _slow[index];
                    slowEffect.Tick(this, Time.deltaTime);
                    if (slowEffect.IsExpired)
                    {
                        _slow.RemoveAt(index);
                    }
                }

                if (_slow.Count == 0)
                {
                    OnRemoveStatusEffect();
                }
            }
            

            if (IsSlowing && !DashConstant)
            {
                SlowFactor = 1 - _slow.Max(s => s.Value);
            }
            else
            {
                SlowFactor = 1f;
            }

            if (IsSlowing)
            {
                if (slowFx && slowFx.isStopped) slowFx.Play();
            }
            else
            {
                if (slowFx && slowFx.isPlaying) slowFx.Stop();
            }
        }
        
        void UpdateBurn()
        {
            BurnIncreaseTakenDamage = 0;
            if (IsBurning)
            {
                for (var index = _burn.Count - 1; index >= 0; index--)
                {
                    if (index >= _burn.Count) continue;
                    var burnEffect = _burn[index];
                    burnEffect.Tick(this, Time.deltaTime);
                    if (_burn.Count > index && burnEffect.IsExpired)
                    {
                        _burn.RemoveAt(index);
                    }
                    else
                    {
                        BurnIncreaseTakenDamage += burnEffect.BurnIncreaseTakenDamage;
                    }
                }

                if (_burn.Count == 0)
                {
                    OnRemoveStatusEffect();
                }
            }
        }
        
        void UpdatePoison()
        {
            PoisonDeBuffDamage = 0;
            if (IsPoisoning)
            {
                for (var index = _poison.Count - 1; index >= 0; index--)
                {
                    if (index >= _poison.Count) continue;
                    var poisonEffect = _poison[index];
                    poisonEffect.Tick(this, Time.deltaTime);
                    if (_poison.Count > index && poisonEffect.IsExpired)
                    {
                        _poison.RemoveAt(index);
                    }
                    else
                    {
                        PoisonDeBuffDamage = Mathf.Max(PoisonDeBuffDamage, poisonEffect.PoisonDeBuffDamage);
                    }
                }

                if (_poison.Count == 0)
                {
                    OnRemoveStatusEffect();
                }
            }
        }
        
        void UpdateBleed()
        {
            BleedIncreaseTakenDamage = 0;
            if (IsBleeding)
            {
                for (var index = _bleed.Count - 1; index >= 0; index--)
                {
                    if (index >= _bleed.Count) continue;
                    var bleedEffect = _bleed[index];
                    bleedEffect.Tick(this, Time.deltaTime);
                    if (_bleed.Count > index && bleedEffect.IsExpired)
                    {
                        _bleed.RemoveAt(index);
                    }
                    else
                    {
                        BleedIncreaseTakenDamage += bleedEffect.BleedIncreaseTakenDamage;
                    }
                }

                if (_bleed.Count == 0)
                {
                    OnRemoveStatusEffect();
                }
            }
        }

        void UpdateAnimatorSpeed()
        {
            Animator.speed = AnimatorSpeed;
        }

        protected virtual Vector3 EvaluatePullForce(Vector3 force, float rawForce, bool canEscape)
        {
            return force;
        }

        protected virtual void InitProperties()
        {
            ResetLastTimeAttack();
            IsPreview = false;
            LastTimeGetHit = 0;
            GetHitCount = 0;
            IsInvisible = false;
            LastTimeTakeDamage = 0;
            IsPlayingAnim = false;
            if (Animator) Animator.enabled = true;
            Attacking = false;
            DealingDamage = false;
            _rootMotionMult = 1;
            Hp = MaxHp;
            _animatorSpeed = 1f;
            SlowFactor = 1f;
            IsSkillMoving = false;
            DashConstant = false;
            _pullHp = MaxPullHp;
            IsDying = false;
#if !DACODER_RELEASE || UNITY_EDITOR
            IsInvincible = false;
#endif
            RaisedDieEvent = false;
            RangedPerfectCount = 0;
            LastTimeRangedPerfect = 0;
            _buffs = new List<BuffData>();
            LastStatusEffect = null;
            CanShowWarning = false;
            _bountyMarkExpiredTime = -1;
            _markCoolDownTime = -1;
            ClearStatusEffects();
            ActorMovement.InitProperties(Config.avoidancePriorityNormal, Config.acceleration);
            _attachSlotDict = new Dictionary<WeaponAttachSlotType, WeaponAttachSlot>();
            _attachSlots = GetComponentsInChildren<WeaponAttachSlot>(true);
            foreach (var slot in _attachSlots)
            {
                _attachSlotDict.Add(slot.slotType, slot);
            }
            SetupModel();
            UnDissolve();
        }
        
        public virtual void SetupModel()
        {
            foreach (var attachSlot in _attachSlots)
            {
                attachSlot.UnBind();
            }
            
            var hitBoxLayer = TeamConfig.GetLayerMask(LayerMaskType.HitBoxLayer).ToGameObjectLayer();
            var enemyHitBoxLayer = TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer).ToGameObjectLayer();
            foreach (var bodyPart in BodyParts)
            {
                if (bodyPart is AllyHitBox)
                {
                    bodyPart.SetLayer(enemyHitBoxLayer);
                    bodyPart.TurnOffHitBox();
                }
                else
                {
                    bodyPart.SetLayer(hitBoxLayer);
                }
            }
            
            var pivotWeaponAttach = GetComponentsInChildren<PivotWeaponAttachSlot>(true);
            foreach (var attachSlot in _attachSlots)
            {
                var pivot = pivotWeaponAttach.FirstOrDefault(p => p.slotType == attachSlot.slotType);
                if (pivot != null)
                {
                    attachSlot.Bind(pivot.transform);
                }
            }

            _bodyParts = _bodyParts.Except(AllyHitBoxes).ToArray();
            if (_animatorEventListener == null)
            {
                AnimatorListenEvent();
            }
            ResetPassiveBuffFx();
        }

        public void SetupParticleSkinnedMesh(SkinnedMeshRenderer mesh)
        {
            foreach (var particleMesh in ParticlesMesh) 
            {
                particleMesh.ApplyMesh(mesh);
            }
        }

        public virtual void SetConfig(ActorConfig config, TeamConfig teamConfig)
        {
            Config = config;
            TeamConfig = teamConfig;
            AddToAllyCollection();
        }

        protected virtual void InitSkills()
        {
        }

        protected abstract void InitStateMachine();

        void ResetPassiveBuffFx()
        {
            if (passiveBuffFx == null) return;
            foreach (var fx in passiveBuffFx)
            {
                fx.ResetFx();
            }
        }

        void ResetLastTimeAttack()
        {
            LastTimeAttack = Time.time;
        }

        void OnSkillBeginHitEvent(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnBeginHit(index);
            }
        }

        void OnSkillStopIndicatorEvent(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnStopIndicator(index);
            }
        }

        protected virtual void OnSkillBeginTrail(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnBeginTrail(index);
            }
        }

        protected virtual void OnSkillStopTrail(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnStopTrail(index);
            }
        }

        void OnSkillBeginMove(int index)
        {
            if (CurrentSkill)
            {
                IsSkillMoving = true;
                ResetKnockBack();
                ActorMovement.OnSkillBeginMove(Config.avoidancePriorityAttack);
                CurrentSkill.OnBeginMove(index);
            }
        }

        void OnSkillStopMove(int index)
        {
            IsSkillMoving = false;
            if (CurrentSkill)
            {
                CurrentSkill.OnStopMove(index);
            }

            ActorMovement.OnSkillStopMove(Config.avoidancePriorityNormal);
        }

        void OnSkillBeginTracking(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnBeginTracking(index);
            }
        }

        void OnSkillStopTracking(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnStopTracking(index);
            }
        }

        void OnSkillPrepareShoot(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnPrepareShoot(index);
            }
        }

        void OnSkillChargeFull(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnChargeFull(index);
            }
        }

        void OnSkillShoot(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnShoot(index);
            }
        }

        void OnEnterLocomotionState()
        {
            IsPlayingAnim = false;
        }

        void OnAnimMove()
        {
            var deltaPosition = Animator.deltaPosition;
            ActorMovement.Move(deltaPosition * _rootMotionMult);

            Animator.transform.localPosition = Vector3.zero;

            if (CurrentSkill)
            {
                CurrentSkill.OnAnimationMove(deltaPosition);
            }
        }

        protected virtual void OnSkillCustomEvent(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnCustomEvent(index);
            }
        }

        protected virtual void OnSkillFeedbackEvent(int index)
        {
            if (CurrentSkill)
            {
                CurrentSkill.OnFeedbackEvent(index);
            }
        }

        void OnStateCustomEvent(int index)
        {
            StateMachine.CurrentState.OnCustomEvent(index);
        }

        protected virtual void OnFoot(int index)
        {
            if (feedback.footFeedback)
            {
                feedback.footFeedback.Play();
            }
        }

        public virtual void OnJumpLanding()
        {
            if (feedback.jumpLandingFeedback)
            {
                feedback.jumpLandingFeedback.Play();
            }
        }

        public virtual void ResetKnockBack()
        {
            DamageForce = new CombinedDamageForceData();
        }

        public virtual DamageConfig BuffModifyDamageConfig(DamageConfig damageConfig)
        {
            foreach (var buff in _buffs)
            {
                damageConfig = buff.ModifyDamageConfig(damageConfig);
            }

            return damageConfig;
        }

        public virtual StatusEffectData[] GetStatusEffectsBuffData(DamageConfig damageConfig)
        {
            return null;
        }

        public void OnCriticalDamage()
        {

        }

        public virtual float GetGambleMultiplier()
        {
            return 0;
        }

        public IDamageTaker GetRandomEnemy(float range = -1, HashSet<IDamageTaker> except = null)
        {
            return EnemyCollection.GetRandom(Position, range, except);
        }

        public HitInfo TakeDamage(CombinedDamageData combinedDamageData)
        {
            if (!Alive || IgnoreDamage || !Initialized)
            {
                if (IgnoreDamage && textSpawner && Alive && Initialized)
                {
                    textSpawner.SpawnStatusText("Immune!", HeadPosition);
                }
                return new HitInfo{Damage = 0, HitType = HitType.None};
            }
            
            return TakeDamageInternal(combinedDamageData);
        }
        
        protected virtual HitInfo TakeDamageInternal(CombinedDamageData combinedDamage)
        {
            if (combinedDamage.MissChance > 0 && Random.value < combinedDamage.MissChance)
            {
                SpawnStatusText("Miss", combinedDamage.DamageForce.HitPosition);
                return new HitInfo { Damage = 0, HitType = HitType.None };
            }

            var damage = combinedDamage;
            if (damage.DamageScaleType == DamageScaleType.ScaleByEnemyMaxHp)
            {
                damage.DamageScaleType = DamageScaleType.Raw;
                damage.Damage = MaxHp * damage.Damage;
            }
            
            if (damage.DamageScaleType == DamageScaleType.ScaleByEnemyHp)
            {
                damage.DamageScaleType = DamageScaleType.Raw;
                damage.Damage = Hp * damage.Damage;
            }
            
            if (!EvaluateDamage(damage, out var finalDamage))
            {
                return new HitInfo{Damage = 0, HitType = HitType.None};
            }

            var hitType = ApplyFinalDamage(finalDamage);
            
            return hitType;
        }
        
        bool EvaluateDamage(CombinedDamageData combinedDamage, out FinalDamageData finalDamage)
        {
            var isCriticalHit = false;
            finalDamage = new FinalDamageData();
            var damageBonus = combinedDamage.DamageBonus;
            if (Random.value < combinedDamage.CriticalChance && (combinedDamage.DamageType.HasFlag(DamageType.Weapon) || combinedDamage.ForceCheckCritical))
            {
                isCriticalHit = true;
                damageBonus += combinedDamage.CriticalDamage;
                combinedDamage.DamageSource.OnCriticalDamage();
            }

            if (combinedDamage.BackDamageBonus > 0)
            {
                var dir = Position - combinedDamage.DamageForce.CenterPosition;
                dir.y = 0;
                dir.Normalize();
                var dot = Vector3.Dot(dir, Transform.forward);
                if (dot >= 0.5f)
                {
                    damageBonus += combinedDamage.BackDamageBonus;
                }
            }
            
            var damage = combinedDamage.Damage * (1 + damageBonus + combinedDamage.DamageCommitedBonus * (1f - HpRatio)) + combinedDamage.AdditionalFlatDamage;
            
            if (!combinedDamage.CanNotEvade && Random.value < Evade)
            {
                SpawnStatusText("Evaded!", LockPosition);
                return false;
            }
            
            var damageReduction = GetDamageReduction();
            damage *= 1 - damageReduction;
            var statusEffect = EvaluateStatusEffect(combinedDamage);

            //damage force
            var damageForceData = EvaluateDamageForce(combinedDamage.DamageForce);
            damage = Mathf.RoundToInt(damage) == 0 ? Mathf.CeilToInt(damage) : damage;
            finalDamage = new FinalDamageData(combinedDamage.DamageSource, this, combinedDamage.DamageType,
                damageForceData, combinedDamage.InstantKill, isCriticalHit, Mathf.RoundToInt(damage),
                statusEffect, !combinedDamage.CanNotFireStatusEvent, combinedDamage.AttackIndexOfSource, combinedDamage.AttackId, combinedDamage.ChargeMultBonus, combinedDamage.ExtendDotDuration);

            return true;
        }
        
        protected virtual float GetDamageReduction()
        {
            return DamageReduction - DamageTaken;
        }
        
        CombinedDamageForceData EvaluateDamageForce(CombinedDamageForceData damageForceData)
        {
            if (damageForceData.OverrideCurrentDamageForce
                || damageForceData.Duration <= 0
                || DamageForce.Type == DamageForceType.None
                || DamageForce.Type == DamageForceType.KnockBack
                || DamageForce.Type == DamageForceType.Knockdown && damageForceData.Type == DamageForceType.Pull
                || DamageForce.Type == DamageForceType.Pull && damageForceData.Type == DamageForceType.Pull)
            {
            }
            
            else
            {
                damageForceData.Type = DamageForceType.None;
            }

            if (damageForceData.AddToCurrentDamageForce)
            {
                damageForceData.Force += DamageForce.Force;
                damageForceData.Duration += DamageForce.Duration;
            }

            return damageForceData;
        }
        
        StatusEffect[] EvaluateStatusEffect(CombinedDamageData combinedDamage)
        {
            var statusEffect = new List<StatusEffect>();
            
            var rd = Random.value;
            // stagger
            if (rd < combinedDamage.StaggerChance)
            {
                statusEffect.Add(new StatusEffect(StatusEffectType.Stagger, StatusEffect.TickStagger, combinedDamage.StaggerDuration, 0f, 0, new DOTEffectData(), combinedDamage.SourceAttackId, combinedDamage.DamageSource));
            }
            //slow
            if (combinedDamage.SlowValue > 0 && rd < combinedDamage.SlowChance)
            {
                statusEffect.Add(new StatusEffect(StatusEffectType.Slow, StatusEffect.TickSlow, combinedDamage.SlowDuration, combinedDamage.SlowValue, 0, new DOTEffectData(), combinedDamage.SourceAttackId, combinedDamage.DamageSource));
            }

            foreach (var statusEffectData in combinedDamage.StatusEffectDatas)
            {
                if (rd < statusEffectData.chance)
                {
                    var tickCount = statusEffectData.type switch
                    {
                        StatusEffectType.Slow => StatusEffect.TickSlow,
                        StatusEffectType.Stagger => StatusEffect.TickStagger,
                        StatusEffectType.Cinch => StatusEffect.TickCinch,
                        StatusEffectType.Blind => StatusEffect.TickBlind,
                        _ => Mathf.FloorToInt(statusEffectData.duration / statusEffectData.tickInterval) + 1,
                    };
                    statusEffect.Add(new StatusEffect(statusEffectData.type, tickCount, statusEffectData.tickInterval, statusEffectData.damageValue, statusEffectData.stackIncrease, statusEffectData.dotEffectData ,combinedDamage.SourceAttackId, combinedDamage.DamageSource));
                }
            }
            
            return statusEffect.ToArray();
        }

        protected StatusEffect[] EvaluateStatusEffectsFromData(StatusEffectData[] statusEffectDatas, IDamageSource sourceData, int sourceId)
        {
            if (statusEffectDatas == null || statusEffectDatas.Length == 0) return null;
            
            var rdChance = Random.value;
            var result = new List<StatusEffect>();
            foreach (var statusEffectData in statusEffectDatas)
            {
                if (rdChance < statusEffectData.chance)
                {
                    var tickCount = statusEffectData.type switch
                    {
                        StatusEffectType.Slow => StatusEffect.TickSlow,
                        StatusEffectType.Stagger => StatusEffect.TickStagger,
                        StatusEffectType.Cinch => StatusEffect.TickCinch,
                        StatusEffectType.Blind => StatusEffect.TickBlind,
                        _ => Mathf.FloorToInt(statusEffectData.duration / statusEffectData.tickInterval) + 1,
                    };
                    result.Add(new StatusEffect(statusEffectData.type, tickCount, statusEffectData.tickInterval, statusEffectData.damageValue, statusEffectData.stackIncrease, statusEffectData.dotEffectData, sourceId, sourceData));
                }
            }

            return result.ToArray();
        }
        
        protected virtual HitInfo ApplyFinalDamage(FinalDamageData finalDamage)
        {
            if (!Alive) return new HitInfo{Damage = 0, HitType = HitType.None};
            
            LastTimeGetHit = Time.time;
            GetHitCount++;
            LastTimeTakeDamage = Time.time;
            var damage = Mathf.RoundToInt(finalDamage.Damage * (1 + FinalDamageTakenBonus));
            
            if (finalDamage.InstantKill)
            {
                damage = Mathf.RoundToInt(MaxHp + 1);
                SpawnStatusText("Instant Kill!", finalDamage.DamageForce.HitPosition);
            }
            else if (damage > 0)
            {
                SpawnDamageText(damage, 0, 0, 0, 0, 0, finalDamage.IsCriticalHit,
                    finalDamage.DamageForce.HitPosition, finalDamage.DamageForce.HitDirection);
            }

            if (damage > 0)
            {
                ReduceHp(damage, finalDamage.DamageSource.Type);
                PlayGetHitFeedback();
                PlayDamagedFx(finalDamage.DamageForce.HitPosition, finalDamage.DamageForce.HitDirection);
                if (!Alive)
                {
                    HandleDie(finalDamage.DamageForce);
                }
                if (finalDamage.DamageSource.Alive && finalDamage.DamageSource != this)
                {
                    finalDamage.DamageSource.OnDealDamageToEnemy(finalDamage.DamageType, finalDamage.AttackIndex, finalDamage.AttackId,
                        damage, finalDamage.ChargeMultBonus, true, this);
                    
                    if (Alive && finalDamage.DamageSource is IDamageTaker damageTaker)
                    {
                        if (DamageReflection > 0)
                        {
                            DealDamageReflection(damageTaker, damage);
                        }
                        
                        if (DamageReflectionByMaxHp > 0)
                        {
                            DealDamageReflectionByMaxHp(damageTaker);
                        }

                        if (DamageReflectionBuff)
                        {
                            DealDamageReflectionBuff(damageTaker);
                        }
                    }

                }
            }
            else if (finalDamage.StatusEffects.Length > 0)
            {
                if (finalDamage.DamageSource.Alive)
                {
                    finalDamage.DamageSource.OnDealDamageToEnemy(finalDamage.DamageType, finalDamage.AttackIndex,
                        finalDamage.AttackId,
                        damage, finalDamage.ChargeMultBonus, true, this);
                }
            }
            if (Alive)
            {
                HandleExtendStatusDotDuration(finalDamage.ExtendDotDuration);
                HandleStatusEffect(finalDamage.StatusEffects, finalDamage.FireStatusEffectEvent, finalDamage.DamageSource.Position);
                if (Alive)
                {
                    HandleDamageForce(finalDamage.DamageType, finalDamage.DamageForce);
                }
            }

            return new HitInfo { Damage = damage, HitType = HitType };
        }

        void HandleExtendStatusDotDuration(float duration)
        {
            if (duration == 0) return;
            foreach (var bleed in _bleed)
            {
                bleed.ExtendDuration(duration);
            }
            
            foreach (var poison in _poison)
            {
                poison.ExtendDuration(duration);
            }
            
            foreach (var burn in _burn)
            {
                burn.ExtendDuration(duration);
            }
        }
        
        protected virtual void HandleDamageForce(DamageType damageType, CombinedDamageForceData damageForce)
        {
            damageForce = EvaluateDamageForceSecond(damageForce);
            switch (damageForce.Type)
            {
                case DamageForceType.None:
                    break;
                case DamageForceType.KnockBack:
                    DamageForce = damageForce;
                    if (CanGetHitAnim)
                    {
                        StateMachine.ChangeState<IIdleState>();
                        if (Alive)
                        {
                            StateMachine.ChangeState<GetHitBaseState>(damageForce);
                        }
                    }
                    break;
                case DamageForceType.Pull:
                    _pullHp -= DamageForce.Duration;
                    DamageForce = damageForce;
                    // if (StateMachine.CurrentState is AIAttackState)
                    // {
                    //     StateMachine.ChangeState<IIdleState>();
                    // }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected virtual CombinedDamageForceData EvaluateDamageForceSecond(CombinedDamageForceData damageForce)
        {
            if (damageForce.Type == DamageForceType.Knockdown)
            {
                damageForce.Duration = Mathf.Max(1.5f, damageForce.Duration + 1);
            }
            if (damageForce.Type == DamageForceType.Pull && !CanBePulled)
            {
                damageForce.Type = DamageForceType.None;
            }
            if (damageForce.Type == DamageForceType.Knockdown && !CanBeKnockDown)
            {
                damageForce.Type = DamageForceType.KnockBack;
                damageForce.Duration = Mathf.Max(0, damageForce.Duration - 1);
            }
            if (damageForce.Type == DamageForceType.KnockBack && !CanBeKnockBack)
            {
                damageForce.Type = DamageForceType.None;
            }

            return damageForce;
        }
        
        protected virtual void DealDamageReflection(IDamageTaker damageTaker, int damage)
        {
            damageTaker.ApplyTrueDamage(DamageReflection * damage, Type);
        }
        
        protected virtual void DealDamageReflectionByMaxHp(IDamageTaker damageTaker)
        {
            damageTaker.ApplyTrueDamage(DamageReflectionByMaxHp * MaxHp, Type);
        }
        
        protected virtual void DealDamageReflectionBuff(IDamageTaker damageTaker)
        {
            
        }
        
        protected virtual void HandleDie(CombinedDamageForceData damageForceData)
        {
            IsDying = true;
            SpreadBurn();
            RaiseDieEvent();
            ClearStatusEffects();
            PrepareDead(damageForceData);
        }

        void SpreadBurn()
        {
            if (_burn.Count <= 0) return;
            var burnSpread = _burn.Where(b => b.DotEffectData.burnCanSpread).ToArray();
            if (burnSpread.Length <= 0) return;
            var hitResults = Physics.OverlapSphere(Position, StatusEffect.BurnSpreadRange, TeamConfig.GetLayerMask(LayerMaskType.HitBoxLayer));
            var hashSet = new HashSet<IDamageTaker>();
            foreach (var hit in hitResults)
            {
                var damageTaker = hit.GetComponentInParent<IDamageTaker>();
                if (damageTaker is { Alive: true } && !hashSet.Contains(damageTaker))
                {
                    hashSet.Add(damageTaker);
                }
            }
        }

        public void TakeBurnSpreadDamage(StatusEffect[] statusEffects)
        {
            if (statusEffects.Length <= 0) return;
            if (_burn.Count > 0)
            {
                foreach (var burn in _burn)
                {
                    burn.ResetCooldown();
                }
            }
            else
            {
                var statusEffectNew = new List<StatusEffect>();
                foreach (var status in statusEffects)
                {
                    var s = new StatusEffect(status.Type, status.TickCount, status.TickInterval, status.Value,
                        status.Stack, status.DotEffectData, status.SourceId, status.DamageSource);
                    statusEffectNew.Add(s);
                }
                HandleStatusEffect(statusEffectNew.ToArray(), false, Position);
            }
        }
        
        protected virtual void PrepareDead(CombinedDamageForceData damageForceData)
        {
            ToDeadState(damageForceData);
        }
        
        protected void RaiseDieEvent()
        {
            if (RaisedDieEvent)
            {
                Debug.LogError($"Die event has been raised!, {name}", gameObject);
                return;
            }
            RaisedDieEvent = true;
        }
        
        protected virtual void ReduceHp(float value, ActorType actorType)
        {
            if (Mathf.Approximately(value, 0)) return;
            var oldHpRatio = HpRatio;
            Hp -= value;
            OnHpReduce(oldHpRatio, HpRatio);
        }
        
        void PlayDamagedFx(Vector3 pos, Vector3 dir)
        {
            if (damageFxMain && Time.time - _lastTimeHitFx > 0.5f)
            {
                _lastTimeHitFx = Time.time;
                var fx = pools.Spawn(damageFxMain);
                fx.transform.position = pos;
                fx.transform.forward = dir == Vector3.zero ? -Transform.forward : dir;
                fx.Play();
                StartCoroutine(IEDespawnFX(fx.gameObject, fx.main.duration));
            }
        }

        IEnumerator IEDespawnFX(GameObject fx, float delay)
        {
            yield return new WaitForSeconds(delay);
            pools.Despawn(fx);
        }
        
        void PlayGetHitFeedback()
        {
            if (feedback.getHitFeedback)
            {
                feedback.getHitFeedback.Play();
            }
        }
        
        public virtual void OnGetHitEnded()
        {
            //ExtendLastTimeAttack();
        }
        
        protected virtual void OnHpReduce(float oldHpRatio, float newHpRatio)
        {
        }
        
        protected void SpawnDamageText(float physic, float fire, float ice, float poison, float electric, float bleed, bool isCritical, Vector3 hitPos, Vector3 hitDir)
        {
            if (Time.time - _lastTimeSpawnDamageText < 0.2f) hitPos += Random.onUnitSphere;
            _lastTimeSpawnDamageText = Time.time;
            if (!textSpawner) return;
            if (IsPlayer)
            {
                if (physic > 0)
                {
                    textSpawner.SpawnPlayerPhysicText(physic, hitPos, hitDir);
                }
                if (fire > 0)
                {
                    textSpawner.SpawnPlayerPhysicText(fire, hitPos, hitDir);
                }
                if (ice > 0)
                {
                    textSpawner.SpawnPlayerPhysicText(ice, hitPos, hitDir);
                }
                if (poison > 0)
                {
                    textSpawner.SpawnPlayerPhysicText(poison, hitPos, hitDir);
                }
                if (electric > 0)
                {
                    textSpawner.SpawnPlayerPhysicText(electric, hitPos, hitDir);
                }if (bleed > 0)
                {
                    textSpawner.SpawnPlayerPhysicText(bleed, hitPos, hitDir);
                }
            }
            else
            {
                if (physic > 0)
                {
               
                    if (isCritical)
                    {
                        textSpawner.SpawnCriticalText(physic, hitPos, hitDir);
                    }
                    else
                    {
                        textSpawner.SpawnPhysicText(physic, hitPos, hitDir);
                    }
                }
                if (fire > 0)
                {
                    textSpawner.SpawnFireText(fire, hitPos, hitDir);
                }
                if (ice > 0)
                {
                    textSpawner.SpawnIceText(ice, hitPos, hitDir);
                }
                if (poison > 0)
                {
                    textSpawner.SpawnPoisonText(poison, hitPos, hitDir);
                }
                if (electric > 0)
                {
                    textSpawner.SpawnElectricText(electric, hitPos, hitDir);
                }
                if (bleed > 0)
                {
                    textSpawner.SpawnBleedText(bleed, hitPos, hitDir);
                }
            }
        }

        public virtual void TakeStatusEffect(CombinedDamageData combinedDamage)
        {
            if (!Alive) return;
            LastTimeGetHit = Time.time;
            LastTimeTakeDamage = Time.time;
            var statusEffect = EvaluateStatusEffect(combinedDamage);
            HandleStatusEffect(statusEffect, !combinedDamage.CanNotFireStatusEvent, combinedDamage.DamageSource.Position);
        }

        public virtual void TakeStatusEffectDamage(StatusEffectType type, float statusEffectDamage)
        {
            if (!Alive) return;
            if (IgnoreDamage) return;

            LastTimeTakeDamage = Time.time;
            
            var damageReduction = Mathf.Min(1f, GetDamageReduction() + DamageDotReduction);
            var damage = Mathf.RoundToInt(statusEffectDamage * (1 - damageReduction));
            
            switch (type)
            {
                case StatusEffectType.None:
                case StatusEffectType.Slow:
                case StatusEffectType.Stagger:
                case StatusEffectType.Cinch:
                case StatusEffectType.Blind:
                    break;
                case StatusEffectType.Poison:
                    damage = damage == 0 ? Mathf.CeilToInt(statusEffectDamage * (1 - damageReduction)) : damage;
                    if (damage > 0) SpawnDamageText(0, 0, 0, damage, 0, 0, false, LockPosition, -Transform.forward);
                    break;
                case StatusEffectType.Burn:
                    damage = damage == 0 ? Mathf.CeilToInt(statusEffectDamage * (1 - damageReduction)) : damage;
                    if (damage > 0) SpawnDamageText(0, damage, 0, 0, 0, 0, false, LockPosition, -Transform.forward);
                    break;
                case StatusEffectType.Bleed:
                    damage = Mathf.RoundToInt(Hp * statusEffectDamage * (1 - damageReduction));
                    damage = Mathf.Max(damage, 1);
                    SpawnDamageText(0, 0, 0, 0, 0, damage, false, LockPosition, -Transform.forward);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            if (damage <= 0) return;
            
            ReduceHp(damage, ActorType.None);
            OnStatusEffectDmgTaken?.Invoke(type, damage);

            if (!Alive)
            {
                HandleDie(new CombinedDamageForceData());
            }
            

            switch (type)
            {
                case StatusEffectType.None:
                    break;
                case StatusEffectType.Slow:
                    break;
                case StatusEffectType.Stagger:
                    break;
                default:
                    break;
            }
        }

        public bool IsUnderStatusEffect(StatusEffectType type)
        {
            if (type.HasFlag(StatusEffectType.Slow) && IsSlowing)
            {
                return true;
            }

            if (type.HasFlag(StatusEffectType.Stagger) && IsStaggering)
            {
                return true;
            }

            return false;
        }

        public int GetStatusEffectCount(StatusEffectType type)
        {
            switch(type)
            {
                
                case StatusEffectType.Slow:
                    return IsSlowing ? 1 : 0;
                case StatusEffectType.Stagger:
                    return IsStaggering ? 1 : 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public float GetStatusEffectProgress(StatusEffectType type)
        {
            switch(type)
            {
                case StatusEffectType.Slow:
                    return IsSlowing ? _slow.Max(s => s.RemainTimeRatio) : 0;
                case StatusEffectType.Stagger:
                    return IsStaggering ? _stagger.RemainTimeRatio : 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ResetStatusEffectCooldown(StatusEffectType type)
        {
        }
        
        public void ApplyTrueDamage(float damage, ActorType actorType)
        {
            if (!Alive || IgnoreDamage || damage <= 0) return;
            ApplyTrueDamageInternal(damage, actorType);
        }
        
        protected virtual void ApplyTrueDamageInternal(float damage, ActorType stickManType)
        {
            LastTimeTakeDamage = Time.time;
            
            var damageInt = damage < 1 ? Mathf.CeilToInt(damage) : Mathf.RoundToInt(damage);
            
            if (damageInt <= 0) return;
            
            SpawnDamageText(damageInt, 0, 0, 0, 0, 0, false, LockPosition, -Transform.forward);

            ReduceHp(damageInt, stickManType);

            // PlayGetHitFeedback();
            
            if (!Alive)
            {
                HandleDie(new CombinedDamageForceData());
            }
            else
            {
                PlayGetHitFeedback();
            }
        }

        public float Heal(HealData healData)
        {
            if (!Alive && !healData.Revive) return 0;

            if (healData.Amount <= 0) return 0;

            Hp = Mathf.Max(0, Hp);
            if (Hp < MaxHp)
            {
                var healAmount = healData.Amount * (1 + HealExtra); 
                if (healData.IsPlaySound && feedback.healFeedback)
                {
                    feedback.healFeedback.Play();
                }
                HealFx(healData.ActiveFx);
                if (healData.HpText)
                {
                    SpawnHealText(healAmount);
                }

                if (healData.ReviveText)
                {
                    SpawnStatusText("Revived", LockPosition);
                }

                Hp = Mathf.Min(MaxHp, Hp + healAmount);
                return healAmount;
            }

            return 0;
        }

        public void StartPeriodicHealing(HealData healData, int healCount, float interval)
        {
            IEnumerator IEHeal()
            {
                int count = 0;
                while (count < healCount)
                {
                    Heal(healData);
                    count++;
                    yield return new WaitForSeconds(interval);
                }
            }

            StartCoroutine(IEHeal());
        }
        protected void HealFx(bool activeFx)
        {
            if (activeFx)
            {
                if (healFoodFx)
                {
                    if (healFoodFx.isPlaying)
                    {
                        healFoodFx.Stop();
                    }

                    healFoodFx.Play();
                }
            }
        }

        protected virtual void SpawnHealText(float amount)
        {
            if (textSpawner)
            {
                textSpawner.SpawnHealText(amount, LockPosition);
            }
        }
        
        protected virtual StatusEffect EvaluateStatusEffect(StatusEffect statusEffect)
        {
            return statusEffect;
        }

        public void ApplyThawStatusEffect(StatusEffect statusEffect)
        {
            HandleStatusEffect(statusEffect, false, Vector3.zero);
        }

        void HandleStatusEffect(StatusEffect statusEffect, bool fireEvent, Vector3 sourcePosition)
        {
            statusEffect = EvaluateStatusEffect(statusEffect);
            if (statusEffect == null || statusEffect.IsExpired) return;

            switch (statusEffect.Type)
            {
                case StatusEffectType.Slow:
                    if (IgnoreDamage || !CanBeSlow) return;
                    var slowEffect = _slow.FirstOrDefault(s =>
                        s.SourceId == statusEffect.SourceId && s.Type == statusEffect.Type);
                    
                    if (slowEffect == null)
                    {
                        _slow.Add(statusEffect);
                    }
                    else
                    {
                        slowEffect.ResetCooldown();   
                    }
                    break;
                case StatusEffectType.Bleed:
                    if (IgnoreDamage) return;
                    var bleedEffect = _bleed.FirstOrDefault(s =>
                        s.SourceId == statusEffect.SourceId && s.Type == statusEffect.Type);
                    
                    if (bleedEffect == null)
                    {
                        _bleed.Add(statusEffect);
                    }
                    else
                    {
                        bleedEffect.IncreaseStack(statusEffect.Stack);
                        bleedEffect.ResetCooldown();   
                    }
                    LastStatusEffect = statusEffect;
                    break;
                case StatusEffectType.Poison:
                    if (IgnoreDamage) return;
                    var poisonEffect = _poison.FirstOrDefault(s =>
                        s.SourceId == statusEffect.SourceId && s.Type == statusEffect.Type);
                    
                    if (poisonEffect == null)
                    {
                        _poison.Add(statusEffect);
                    }
                    else
                    {
                        poisonEffect.IncreaseStack(statusEffect.Stack);
                        poisonEffect.ResetCooldown();   
                    }
                    LastStatusEffect = statusEffect;
                    break;
                case StatusEffectType.Burn:
                    if (IgnoreDamage) return;
                    var burnEffect = _burn.FirstOrDefault(s =>
                        s.SourceId == statusEffect.SourceId && s.Type == statusEffect.Type);
                    
                    if (burnEffect == null)
                    {
                        _burn.Add(statusEffect);
                    }
                    else
                    {
                        burnEffect.IncreaseStack(statusEffect.Stack);
                        burnEffect.ResetCooldown();   
                    }
                    LastStatusEffect = statusEffect;
                    break;
                case StatusEffectType.Stagger:
                    if (IgnoreDamage || !CanBeStagger) return;
                    StopMovement();
                    if (_stagger == null || _stagger.RemainTimeRatio * _stagger.TickInterval < statusEffect.TickInterval)
                    {
                        _stagger = statusEffect;
                    }

                    if (Alive)
                    {
                        StateMachine.ChangeState<StaggerBaseState>();
                    }
                    LastTimeFreezeOrStaggerOrFear = Time.time;
                    break;
                case StatusEffectType.Cinch:
                    if (IgnoreDamage || !CanBeCinch) return;
                    StopMovement();
                    if (_cinch == null || _cinch.RemainTimeRatio * _cinch.TickInterval < statusEffect.TickInterval)
                    {
                        _cinch = statusEffect;
                    }
                    break;
                case StatusEffectType.Blind:
                    if (IgnoreDamage) return;
                    if (_blind == null || _blind.RemainTimeRatio * _blind.TickInterval < statusEffect.TickInterval)
                    {
                        _blind = statusEffect;
                    }
                    break;
                default:
                    break;
            }
        }
        
        void HandleStatusEffect(StatusEffect[] statusEffects, bool fireEvent, Vector3 sourcePosition)
        {
            if (statusEffects == null || statusEffects.Length == 0) return;

            for (var i = 0; i < statusEffects.Length; i++)
            {
                HandleStatusEffect(statusEffects[i], fireEvent, sourcePosition);
            }
        }

        public virtual void OnDealDamageToEnemy(DamageType damageType, int attackIndex, int attackId, float damage, float chageMultBonus,
            bool canLifeSteal, IDamageTaker damageTaker)
        {
            LastTimeDealDamage = Time.time;
        }
        public virtual void RechargeSkillByPercent(float percent)
        {
        }

        public DamageSourceData GetDamageSourceData()
        {
            return new DamageSourceData(
                this,
                CriticalChanceBonus,
                CriticalDamageBonus,
                MissChance,
                DamageBonus,
                FinalDamageBonus,
                WeaponDamageBonus,
                FinalWeaponDamageBonus,
                -1,
                0,
                SlowDurationBonus,
                StaggerDurationBonus,
                -1,
                false,
                GetGambleMultiplier(),
                StaggerChanceBonus
            );
        }

        public virtual Vector3 GetAimPosition()
        {
            return AimedEnemy?.LockPosition ?? Transform.position + AimedDirection * GetCurrentOrNextSkill().Range;
        }

        public void EnableMovementCollision()
        {
            ActorMovement.EnableMovementCollision();
        }

        public void DisableMovementCollision()
        {
            ActorMovement.DisableMovementCollision();
        }

        public void ChangeMovementCollisionRadius(float radius)
        {
            ActorMovement.ChangeRadius(radius);
        }

        public void TurnOffHitBox()
        {
            foreach (var bodyPart in BodyParts)
            {
                bodyPart.TurnOffHitBox();
            }
        }

        public void TurnOnHitBox()
        {
            foreach (var bodyPart in BodyParts)
            {
                bodyPart.TurnOnHitBox();
            }
        }

        public virtual Vector3 GetMoveDirection()
        {
            return Transform.forward;
        }

        public float GetEnemyDistance()
        {
            return AimedEnemy != null ? SimpleMath.DistXZ(Position, AimedEnemy.Position) : 1000;
        }

        public bool IsEnemyStunned()
        {
            return AimedEnemy is { IsStaggering: true };
        }

        public virtual IDamageTaker GetNearestEnemy(float range, HashSet<IDamageTaker> except = null)
        {
            NearestTarget = EnemyCollection.GetNearest(out _, Position, range, except);

            return NearestTarget;
        }

        public bool IsEnemyInAttackRange(float offset = 0)
        {
            var skill = GetCurrentOrNextSkill();
            return skill && skill.Range + offset > GetEnemyDistance();
        }

        public void Warp(Vector3 position)
        {
            ActorMovement.Warp(position);
        }

        public void MovePosition(Vector3 position, float speed, float rotateSpeed, float stoppingDistance)
        {
            if (IsKnockingBack || IsPulling) return;
            ActorMovement.MovePosition(position, speed, rotateSpeed, stoppingDistance);
        }

        public void StopMovement()
        {
            ActorMovement.Stop();
        }

        public void DisableMovement()
        {
            ActorMovement.DisableMovement();
        }

        public void EnableMovement()
        {
            ActorMovement.EnableMovement();
        }

        public void BeginShowWarning()
        {
            CanShowWarning = true;
        }

        public void RotateDirection(Vector3 direction, float rotateSpeed, bool immediately = false)
        {
            ActorMovement.Rotate(direction, rotateSpeed, immediately);
        }

        public void SetAnimatorSpeed(float speed = 1f)
        {
            _animatorSpeed = speed;
            UpdateAnimatorSpeed();
        }

        public void OverrideAnimator(AnimatorOverrideController animatorOverrideController)
        {
            Animator.runtimeAnimatorController = animatorOverrideController;
        }

        public void SetRootMotionMult(float mult)
        {
            _rootMotionMult = mult;
        }

        public virtual void StartDash()
        {
            if (dashFx)
            {
                dashFx.Clear();
                if (dashFx.isStopped)
                {
                    dashFx.Play();
                }
            }
        }

        public virtual void StopDash()
        {

        }

        public void PlayAnimation(string animName, int layer, float speed, bool fade, bool isLocomotion)
        {
            IsPlayingAnim = !isLocomotion;
            _animatorSpeed = speed;
            UpdateAnimatorSpeed();
            if (!fade)
            {
                Animator.Play(animName, layer, 0);
            }
            else
            {
                Animator.CrossFadeInFixedTime(animName, 0.1f, layer);
            }
        }

        public void UpdateLocomotion(Vector2 vel)
        {
            Animator.SetFloat(verticalHash, vel.y);
            Animator.SetFloat(horizontalHash, vel.x);
        }

        public void UpdateLocomotion(Vector2 vel, float deltaTime)
        {
            Animator.SetFloat(verticalHash, vel.y, 0.1f, deltaTime);
            Animator.SetFloat(horizontalHash, vel.x, 0.1f, deltaTime);
        }

        public void UpdateHitAnimDirection(Vector2 vel)
        {
            Animator.SetFloat("Hit_Vertical", vel.y);
            Animator.SetFloat("Hit_Horizontal", vel.x);
        }

        public void StopCurrentSkill()
        {
            if (CurrentSkill)
            {
                CurrentSkill.Stop();
                CurrentSkill = null;
            }
            OnStopCurrentSkill();
            TurnOnWeapon();
            TurnOffRangeWeapon();
        }

        protected virtual void OnStopCurrentSkill()
        {
            
        }

        public void OnSkillBeginAttack()
        {
            if (CurrentSkill)
            {
                Attacking = true;
                ResetLastTimeAttack();
            }
        }

        public virtual void OnSkillEndAttack()
        {
            if (CurrentSkill)
            {
                if (Attacking)
                {
                    CurrentSkill.OnEndAttack();
                    if (CurrentSkill.TurnOffWeapon) TurnOnWeapon();
                    TurnOffRangeWeapon();
                    ResetLastTimeAttack();
                }
            }

            Attacking = false;
            CanShowWarning = false;
            DealingDamage = false;
        }

        public virtual void OnExitSkillState()
        {
            if (Attacking)
            {
                ResetLastTimeAttack();
            }

            Attacking = false;
            DealingDamage = false;
            CanShowWarning = false;
            IsSkillMoving = false;
            ActorMovement.OnSkillStopMove(Config.avoidancePriorityNormal);
        }

        public virtual void OnSkillStopped(Skill skill)
        {
        }

        public void OnSkillCanMoveNextSkill()
        {
            if (CurrentSkill && CanSetMoveNextSkill)
            {
                IsCanMoveNextSkill = true;
                CurrentSkill.OnCanMoveNextSkill();
            }
        }
        
        public void SpawnSkillText(string s, Vector3 position)
        {
            if (textSpawner == null) return;
            textSpawner.SpawnSkillText(s, position);
        }

        public void SpawnExpText(double value)
        {
            if (textSpawner == null) return;
            textSpawner.SpawnExpText(value, HeadPosition);
        }
        public List<IDamageTaker> GetEnemyInRadius(Vector3 position, float range)
        {
            var results = new List<IDamageTaker>();
            for (var i = EnemyCollection.Count - 1; i >= 0; i--)
            {
                if (EnemyCollection[i].CanBeTarget && SimpleMath.InRange(position, EnemyCollection[i].Position, range))
                {
                    results.Add(EnemyCollection[i]);
                }
            }

            return results;
        }

        public void MoveDirection(Vector3 direction, float speed)
        {
            if (IsKnockingBack || IsPulling) return;
            ActorMovement.MoveDirection(direction, speed);
        }

        public virtual void OnRevive()
        {
            if (feedback.reviveFeedback)
            {
                feedback.reviveFeedback.Play();
            }
            TurnOnHitBox();
        }

        public void ToIdleState()
        {
            StateMachine.ChangeState<IIdleState>();
        }

        public virtual void ToDeadState(CombinedDamageForceData combinedDamageForceData)
        {
            StateMachine.ChangeState<DeadBaseState>(combinedDamageForceData);
        }

        public void InstantDead(CombinedDamageForceData combinedDamageForceData)
        {
            if (!Alive) return;
            InstantDeadInternal(combinedDamageForceData);
        }
        protected virtual void InstantDeadInternal(CombinedDamageForceData damageForceData)
        {
            Hp = 0;
            HandleDie(damageForceData);
        }
        public virtual void WakeUpAttack()
        {
        }
        
        public virtual void OnDead()
        {
            TurnOffHitBox();
        }

        public void PlayTrailFx()
        {
            if (trailFx && !trailFx.isPlaying)
            {
                trailFx.Play();
            }
        }

        public void StopTrailFx()
        {
            if (trailFx && trailFx.isPlaying)
            {
                trailFx.Stop();
            }
        }

        public void PlayPassiveBuffFx(BuffType buffType)
        {
            if (passiveBuffFx == null) return;
            var fx = passiveBuffFx.FirstOrDefault(fx => fx.Type == buffType);
            if (fx != null) fx.Play();
        }

        public void StopPassiveBuffFx(BuffType buffType)
        {
            if (passiveBuffFx == null) return;
            var fx = passiveBuffFx.FirstOrDefault(fx => fx.Type == buffType);
            if (fx != null) fx.Stop();
        }

        public void OnBeginRoll()
        {
            if (feedback.rollFeedback)
            {
                feedback.rollFeedback.Play();
            }
        }

        public void OnStopRoll()
        {
        }

        public void SpawnStatusText(string text, Vector3 position)
        {
            if (textSpawner)
            {
                textSpawner.SpawnStatusText(text, position);
            }
        }

        public void TriggerDespawnEvent()
        {
            OnDespawnEvent?.Invoke(this);
        }
        
        public virtual void Despawn(Vector3 position)
        {
            TriggerDespawnEvent();
            pools.Despawn(gameObject);
        }

        public void AddBuff(BuffData buff)
        {
            if (!_buffs.Contains(buff))
            {
                _buffs.Add(buff);
                OnBuffChangedEvent?.Invoke();
            }
        }

        public void RemoveBuff(BuffData buff)
        {
            if (_buffs.Contains(buff))
            {
                _buffs.Remove(buff);
                OnBuffChangedEvent?.Invoke();
            }
        }

        public bool IsBuffActivated(BuffData.BuffType buffType)
        {
            return _buffs.Any(b => b.type == buffType);
        }

        public virtual void ApplyMaterial(bool isInvisible)
        {
        }
        
        public void OnBreakEnded()
        {
            ExtendLastTimeAttack();
        }
        
        protected virtual void ExtendLastTimeAttack()
        {
        }

        void TriggerHpChanged(float oldValue, float newValue)
        {
            OnHpChanged?.Invoke(oldValue, newValue);
        }

        void AddToAllyCollection()
        {
            if (TeamConfig != null && TeamConfig.allyCollection != null && !TeamConfig.allyCollection.Contains(this))
            {
                TeamConfig.allyCollection.Add(this);
            }
        }

        protected void RemoveFromAllyCollection()
        {
            if (TeamConfig != null && TeamConfig.allyCollection != null && TeamConfig.allyCollection.Contains(this))
            {
                TeamConfig.allyCollection.Remove(this);
            }
        }

        protected virtual void ClearStatusEffects()
        {
            IsIgnoreKnockBack = false;
            ClearNegativeStatusEffects();
        }

        public void ClearNegativeStatusEffects()
        {
            _slow = new List<StatusEffect>();
            SlowFactor = 1;
            // _freeze = null;
            // _freezeFactor = 1;
            _stagger = null;
            _cinch = null;
            _blind = null;
            _bleed = new List<StatusEffect>();
            _burn = new List<StatusEffect>();
            _poison = new List<StatusEffect>();
            BleedIncreaseTakenDamage = 0;
            BurnIncreaseTakenDamage = 0;
            PoisonDeBuffDamage = 0;
            
            OnRemoveStatusEffect();

            ResetFx(slowFx);
            ResetFx(stunFx);
            ResetFx(criticalFx);
            ResetFx(dashFx);
            ResetFx(dissolveFx);
            ResetKnockBack();
        }

        void OnRemoveStatusEffect()
        {
            OnStatusEffectChanged?.Invoke();
        }

        protected void ResetFx(ParticleSystem fx)
        {
            if (!fx) return;
            fx.Stop();
            fx.gameObject.SetActive(false);
            fx.gameObject.SetActive(true);
        }

        public void UseSkill(Skill skill, int attackIndex = -1, int inputIndex = -1, Action<bool> isDealDamage = null, Action<int> dealDamageCount = null)
        {
            StopCurrentSkill();
            CurrentSkill = skill;
            IsCanMoveNextSkill = false;
            if (CurrentSkill.TurnOffWeapon)
            {
                TurnOffWeapon();
            }
            else
            {
                TurnOnWeapon();
            }

            if (CurrentSkill.TurnOnRangeWeapon)
            {
                TurnOnRangeWeapon();
            }
            else
            {
                TurnOffRangeWeapon();
            }
            
            OnUseCurrentSkill();
            CurrentSkill.Use(attackIndex, inputIndex, isDealDamage, dealDamageCount);
        }
        
        protected virtual void OnUseCurrentSkill()
        {
            
        }

        protected virtual int GetAttackIndex()
        {
            return -1;
        }
        
        public void OnAppear()
        {
            if (Config.appearFxPrefab)
            {
                var pos = FXPosition;
                var fx = pools.Spawn(Config.appearFxPrefab);
                fx.transform.position = pos;
                fx.transform.localScale = Vector3.one * Config.appearFxScale;
                fx.Play();
            }
            if (feedback.appearFeedback)
            {
                feedback.appearFeedback.Play();
            }
            if (Alive)
            {
                StateMachine.ChangeState<AppearBaseState>();
            }
        }

        public WeaponAttachSlot FindAttachSlot(WeaponAttachSlotType configSlot)
        {
            return _attachSlotDict[configSlot];
        }

        public WeaponAttachSlot[] GetAllAttachSlot()
        {
            return _attachSlots;
        }
        
        public void TurnOffWeapon()
        {
            if (Weapon)
            {
                Weapon.TurnOffVisual();
            }
        }

        public void TurnOnWeapon()
        {
            if (Weapon)
            {
                Weapon.TurnOnVisual();
            }
        }
        
        public void TurnOnRangeWeapon()
        {
            if (RangeWeapon)
            {
                RangeWeapon.TurnOnVisual();
            }
        }
        
        public void TurnOffRangeWeapon()
        {
            if (RangeWeapon)
            {
                RangeWeapon.TurnOffVisual();
            }
        }
        
        public virtual void OnKnockDownRaising()
        {
        }

        public void OnKnockDownEnded()
        {
            ExtendLastTimeAttack();
        }
        
        public virtual Skill GetCurrentOrNextSkill()
        {
            return CurrentSkill ? CurrentSkill : GetNextWeaponSkill();
        }

        protected virtual Skill GetNextWeaponSkill()
        {
            return Weapon ? Weapon.GetNextSkill() : null;
        }
        
        public void RestoreStaggerAfterKnockdown()
        {
            if (Alive && IsStaggering)
            {
                var remainTime = _stagger.TickInterval - _stagger.CurrentTickTime;
                if (remainTime < 1)
                {
                    _stagger.TickInterval = 1;
                    _stagger.ResetCooldown();
                }
                if (Alive)
                {
                    StateMachine.ChangeState<StaggerBaseState>();
                }
            }
        }
        
        protected virtual IDamageTaker GetAimedEnemy(Vector3 forward, float range, bool followAngle = false)
        {
            var s = GetCurrentOrNextSkill();
            if (s == null) return null;

            var minAngle = GetCurrentOrNextSkill().AimAssistAngle;
            if (minAngle <= 0) return null;
            
            AimedTarget = GetAimedEnemyInternal(forward, range, minAngle, followAngle);

            return AimedTarget;
        }
        
        protected IDamageTaker GetAimedEnemyInternal(Vector3 forward, float range, float minAngle, bool followAngle = false, bool includeAlly = false, bool allyAimed = false)
        {
            AimedTarget = null;
            var sqrRange = range * range;
            var minRange = sqrRange;
            
            var forwardNorm = forward.normalized;
            var minDot = Mathf.Cos(minAngle * Mathf.Deg2Rad);
            var maxDot = -1f;
            
            if (!allyAimed)
            {

                for (var i = EnemyCollection.Count - 1; i >= 0; i--)
                {
                    if (EnemyCollection[i].CanBeTarget)
                    {
                        var dir = EnemyCollection[i].LockPosition - Position;
                        var dirNorm = dir.normalized;
                        var dot = Vector3.Dot(forwardNorm, dirNorm);
                        //dir.y = 0;
                        //var ang = Vector3.Angle(forward, dir);
                        var ran = dir.sqrMagnitude;
                        
                        if (dot <= minDot) continue;
                        
                        if (followAngle)
                        {
                            if (dot > maxDot && ran < minRange)
                            {
                                AimedTarget = EnemyCollection[i];
                                maxDot = dot;
                            }
                        }
                        else
                        {
                            if (dot > maxDot && ran < minRange)
                            {
                                AimedTarget = EnemyCollection[i];
                                minRange = ran;
                            }
                        }
                    }
                }

                if (includeAlly)
                {
                    for (var i = AllyCollection.Count - 1; i >= 0; i--)
                    {
                        if ((Actor)AllyCollection[i] == this) continue;
                        if (AllyCollection[i].CanBeTarget)
                        {
                            var dir = AllyCollection[i].LockPosition - Position;
                            var dirNorm = dir.normalized;
                            var dot = Vector3.Dot(forwardNorm, dirNorm);
                            //dir.y = 0;
                            //var ang = Vector3.Angle(forward, dir);
                            var ran = dir.sqrMagnitude;

                            if (dot <= minDot) continue;
                            
                            if (followAngle)
                            {
                                if (dot > maxDot && ran < minRange)
                                {
                                    AimedTarget = AllyCollection[i];
                                    maxDot = dot;
                                }
                            }
                            else
                            {
                                if (dot > maxDot && ran < minRange)
                                {
                                    AimedTarget = AllyCollection[i];
                                    minRange = ran;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (var i = AllyCollection.Count - 1; i >= 0; i--)
                {
                    if ((Actor)AllyCollection[i] == this) continue;
                    if (AllyCollection[i].CanBeTarget)
                    {
                        var dir = AllyCollection[i].LockPosition - Position;
                        var dirNorm = dir.normalized;
                        var dot = Vector3.Dot(forwardNorm, dirNorm);
                        //dir.y = 0;
                        //var ang = Vector3.Angle(forward, dir);
                        var ran = dir.sqrMagnitude;
                        
                        if (dot <= minDot) continue;

                        if (followAngle)
                        {
                            if (dot > maxDot && ran < minRange)
                            {
                                AimedTarget = AllyCollection[i];
                                maxDot = dot;
                            }
                        }
                        else
                        {
                            if (dot > maxDot && ran < minRange)
                            {
                                AimedTarget = AllyCollection[i];
                                minRange = ran;
                            }
                        }
                    }
                }

                AimedTarget ??= this;
            }

            return AimedTarget;
        }
        

        public virtual void OnWeaponHitCount(int hitCount)
        {
            
        }

        public virtual void OnWeaponHitSkill(bool isHit)
        {
            
        }

        protected virtual void OnUseWeaponSkill()
        {
            
        } 
        
        
        public virtual void DeadExplode()
        {
            PlayDeadFx();
            Despawn(Position);
        }

        void PlayDeadFx()
        {
            if (deadFxMain)
            {
                var fx = pools.Spawn(deadFxMain);
                fx.transform.position = FXPosition;
                fx.Play();
            }

            // if (deadFxGround)
            // {
            //     var pos = Position;
            //     if (pos.y < 0.3f && pos.y > -0.3f)
            //     {
            //         pos.y = 0.1f;
            //         var fx = pools.Spawn(deadFxGround);
            //         fx.transform.position = pos;
            //         fx.Play();
            //     }
            // }

            PlayDeadFeedback();
        }
        public void Dissolve(float duration, bool reverse = false)
        {
            if (dissolveFx != null)
            {
                dissolveFx.Clear();
                dissolveFx.Play();
            }
            
            DOTween.To(() => 0, x =>
            {
                for (var i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material.SetFloat("_CutOff", reverse ? 1 - x : x);
                }
            }, 1f, duration).SetTarget(this).Play();
        }
        
        void UnDissolve()
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_CutOff", 0);
            }
        }

        public void EnterStealth()
        {
            if (stealthFx != null)
            {
                var fx = pools.Spawn(stealthFx);
                fx.transform.position = FXPosition;
                fx.Clear();
                fx.Play();
            }
            
            var mbp = new MaterialPropertyBlock();
            foreach (var renderer in renderers)
            {
                renderer.GetPropertyBlock(mbp);
                mbp.SetFloat("_GhostValue", 0f);
                renderer.SetPropertyBlock(mbp);
            }

            IsInvisible = true;
            feedback?.stealthEnterFeedback?.Play();
        }

        public void ExitStealth()
        {
            if (stealthFx != null)
            {
                var fx = pools.Spawn(stealthFx);
                fx.transform.position = FXPosition;
                fx.Clear();
                fx.Play();
            }
            var mbp = new MaterialPropertyBlock();
            foreach (var renderer in renderers)
            {
                renderer.GetPropertyBlock(mbp);
                mbp.SetFloat("_GhostValue", 1f);
                renderer.SetPropertyBlock(mbp);
            }
            IsInvisible = false;
            feedback?.stealthExitFeedback?.Play();
        }

        public void ToggleRunTraceFX(bool state)
        {
            if (runTraceFX != null)
            {
                if (state)
                {
                    runTraceFX.Clear();
                    runTraceFX.Play();
                }
                else
                {
                    runTraceFX.Stop();
                }
            }
        }
        
        public void PlayDeadFeedback()
        {
            if (feedback && feedback.deadFeedback)
            {
                feedback.deadFeedback.Play();
            }
        }
        public void TurnOffVisual()
        {
            TurnOffVisualInternal();
        }

        public void TurnOnVisual()
        {
            TurnOnVisualInternal();
        }

        public virtual void CheckOverrideAnimBySkin(string weaponId)
        {
        }

        protected virtual void TurnOffVisualInternal()
        {
            foreach (var r in renderers)
            {
                r.enabled = false;
            }

            if (visualGo != null)
            {
                foreach (var go in visualGo)
                {
                    go.SetActive(false);
                }
            }
        }

        protected virtual void TurnOnVisualInternal()
        {
            foreach (var r in renderers)
            {
                r.enabled = true;
            }
            if (visualGo != null)
            {
                foreach (var go in visualGo)
                {
                    go.SetActive(true);
                }
            }
        }

        public void RefreshBountyMark(float timeExpired)
        {
            if (!CanBeMark) return;
            if (_bountyMarkExpiredTime > timeExpired) return;
            _bountyMarkExpiredTime = timeExpired;
        }

        public void ResetBountyMarkCoolDown(float coolDownTime)
        {
            _bountyMarkExpiredTime = -1;
            if (_markCoolDownTime >= coolDownTime) return;
            _markCoolDownTime = coolDownTime;
        }

        [Sirenix.OdinInspector.Button]
        void Test()
        {
            if (this is AIEnemy aiEnemy)
            {
                Debug.Log((StateMachine.CurrentState, aiEnemy.CurrentSkill, aiEnemy.AimedEnemy, aiEnemy.Animator.GetCurrentAnimatorStateInfo(0).IsName("locomotion")));
            }
            
        }
    }
}