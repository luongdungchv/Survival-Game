using System;
using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoxHead2.Skills
{
    public abstract class Skill : BaseSO, ISubActionDataProvider<Skill>
    {
        [SerializeField] public string title;
        [SerializeField] public SkillType type;
        [SerializeField] public Sprite icon;
        [SerializeField, Multiline] public string desc;
        [SerializeField] bool triggerChanged;
        [SerializeField] bool ignoreDamageForce;
        [SerializeField] bool canRecast;
        [SerializeField] bool canRollCancel = false;
        [SerializeField] bool turnOffWeapon = false;
        [SerializeField] bool turnOnRangeWeapon = false;
        [SerializeField] bool canInvisible = false;
        [SerializeField] float aimAssistAngle = 180f;
        [SerializeField] float cooldown;
        [SerializeField] bool needCoolDownFirstTime;
        [SerializeField] float range = 10f;
        [SerializeField] float radius = 0.5f;
        [SerializeField] protected float rootMotionMult = 1;
        [SerializeField] SkillPartialAction[] skillPartialActions;
        [SerializeField] float delayAttack;
        [SerializeField] bool notCoolDownWhenPerforming;
        [SerializeField] bool canPerformingUseSkill;
        [SerializeField] private float rangeBonus;
        [SerializeField] private float radiusBonus;
        
        [Header("Special bonus")] [SerializeField]
        float _reduceCoolDown;

        public event System.Action OnChangedEvent;

        public float AimAssistAngle => aimAssistAngle;
        public virtual bool IncludeAlly => false;
        public virtual bool AllyAimed => false;
        public bool Usable => true;
        public bool Available => !Performing && (ChargeCount > 0 || Cooldown <= 0);

        public float Cooldown => NoCooldown ? 0 : (cooldown <= 0
            ? cooldown
            : cooldown * (1 + CooldownBonus + this._reduceCoolDown) -
              CooldownReduceFlat);
        public virtual float CooldownReduceFlat => 0;
        public float ElapsedTime { get; private set; }
        public float CooldownRatio { get; private set; }
        public int ChargeCount { get; protected set; }
        public int MaxChargeCount => 1;
        public bool Performing { get; protected set; }
        public int AttackIndex { get; private set; }
        public int SourceAttackId { get; set; }
#if !DACODER_RELEASE || UNITY_EDITOR
        public bool NoCooldown { get; set; }
        #else
        public bool NoCooldown => false;
#endif
        public int InputIndex { get; private set; }
        public bool IsFinalStopConditionMet => IsStopConditionMet; 
        public abstract bool IsStopConditionMet { get; }
        public bool IsCanMoveNextSkill { get; protected set; }
        protected float RangeBonus => rangeBonus;
        public virtual float Range => range * (1 + RangeBonus);
        public float AimRange => range * (1 + RangeBonus);
        protected float RadiusBonus => radiusBonus;
        public virtual float Radius => radius * (1+ RadiusBonus);
        public IActor Actor { get; set; }
        public bool TurnOffWeapon => turnOffWeapon;
        public virtual bool IsAimFollowAngle => false;
        public bool TurnOnRangeWeapon => turnOnRangeWeapon;
        public virtual bool TurnOffVisual => false;
        public bool CanInvisible => canInvisible;
        public bool CanRecast
        {
            get => canRecast;
            set => canRecast = value;
        }
        public bool CanRollCancel => canRollCancel;
        public bool IgnoreDamageForce => ignoreDamageForce;
        public virtual bool CanPerformingUseSkill => canPerformingUseSkill;
        public float DamageBonus { get; set; }
        public virtual float CooldownBonus { get; set; }
        protected bool UseCachedInput { get; set; }
        protected Vector3 CachedAimedDirection { get; set; }
        protected Vector3 CachedAimedPosition { get; set; }
        protected IDamageTaker CachedAimedEnemy { get; set; }

        Coroutine _performRoutine;
        Coroutine _delayCoroutine;
        float _delayTime;

        public static int SkillIndex = 0;
        
        protected Action<bool> _isDealDamage;
        protected Action<int> _dealDamageCount;

        public override T CreateCopy<T>()
        {
            var copy = base.CreateCopy<Skill>();
            copy.Performing = false;
            copy.AttackIndex = -1;
            copy.ChargeCount = 0;
            copy.DamageBonus = 0;
            copy.CooldownBonus = 0;
            copy._reduceCoolDown = 0;
            if (skillPartialActions != null)
            {
                copy.skillPartialActions = new SkillPartialAction[skillPartialActions.Length];
                for (var i = 0; i < skillPartialActions.Length; i++)
                {
                    copy.skillPartialActions[i] = skillPartialActions[i].CreateCopy<SkillPartialAction>();
                }
            }

            copy.SourceAttackId = ++SkillIndex;
            copy.CheckCoolDownFirstTime();
            return copy as T;
        }

        public void Attach(IActor actor)
        {
            Actor = actor;
            OnAttach();
        }

        protected virtual void OnAttach()
        {
            // if (!needCoolDownFirstTime)
            // {
            //     ElapsedTime = Cooldown;
            // }
            if (skillPartialActions != null)
            {
                for (var i = 0; i < skillPartialActions.Length; i++)
                {
                    skillPartialActions[i].Attach(this);
                }
            }
        }

        void OnReduceCoolDownParamChanged(float oldValue, float newValue)
        {
            _reduceCoolDown = newValue;
        }

        public void CheckCoolDownFirstTime()
        {
            if (!needCoolDownFirstTime)
            {
                ElapsedTime = Cooldown;
            }
        }

        public void Detach()
        {
            CooldownBonus = 0;
            OnDetach();
        }

        protected virtual void OnDetach()
        {
            if (skillPartialActions != null)
            {
                for (var i = 0; i < skillPartialActions.Length; i++)
                {
                    skillPartialActions[i].Detach(this);
                }
            }

            Stop();
        }

        public void SetCooldown(float skillCooldown, int skillCharge)
        {
            ElapsedTime = skillCooldown;
            ChargeCount = skillCharge;
            ValidateCooldown();
            TriggerChanged();
        }

        public void ReduceCooldownPercent(float percent)
        {
            if (Performing && notCoolDownWhenPerforming) return;
            ElapsedTime += Cooldown * percent;
            ValidateCooldown();
            TriggerChanged();
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (Performing && notCoolDownWhenPerforming) return;
            ElapsedTime += deltaTime;
            ValidateCooldown();
            TriggerChanged();
        }

        void ValidateCooldown()
        {
            if (Cooldown == 0)
            {
                ElapsedTime = 0;
                ChargeCount = MaxChargeCount;
                CooldownRatio = 0;
            }
            else
            {
                if (ElapsedTime >= Cooldown)
                {
                    var time = Mathf.FloorToInt(ElapsedTime / Cooldown);
                    ChargeCount = Mathf.Max(0, ChargeCount) + time;
                    ElapsedTime -= time * Cooldown;
                }
                CooldownRatio = Mathf.Max(Cooldown - ElapsedTime, 0f) / Cooldown;
                if (ChargeCount >= MaxChargeCount)
                {
                    ElapsedTime = 0;
                    ChargeCount = MaxChargeCount;
                    CooldownRatio = 0;
                }
            }
        }

        public virtual DamageSourceData GetDamageSourceData()
        {
           var damageSource = (Actor as IDamageSource).GetDamageSourceData();
           damageSource.AttackIndex = AttackIndex;
           damageSource.DamageBonus += DamageBonus;
           damageSource.SourceAttackId = SourceAttackId;
           return damageSource;
        }
        
        protected Vector3 GetAimedDirection()
        {
            return UseCachedInput ? CachedAimedDirection : Actor.AimedDirection;
        }

        protected virtual Vector3 GetAimedPosition()
        {
            return UseCachedInput ? CachedAimedPosition : Actor.GetAimPosition();
        }

        public virtual IDamageTaker GetAimedEnemy()
        {
            return UseCachedInput ? CachedAimedEnemy : Actor.AimedEnemy;
        }

        public void Use(int attackIndex, int inputIndex, Action<bool> isDealDamage = null, Action<int> dealDamageCount = null)
        {
            if (Performing) return;
            Performing = true;
            AttackIndex = attackIndex;
            InputIndex = inputIndex;
            IsCanMoveNextSkill = false;
            _isDealDamage = isDealDamage;
            _dealDamageCount = dealDamageCount;
            ResetCachedInput();
            PrepareSkill();
            Perform();
            TriggerChanged();
        }

        protected virtual void PrepareSkill()
        {
        }

        void Perform()
        {
            StopPerformRoutine();
            _delayCoroutine = Actor.StartCoroutine(IEDelay());
            // performRoutine = Actor.StartCoroutine(IEPerform());
        }

        IEnumerator IEDelay()
        {
            _delayTime = delayAttack;
            if (delayAttack > 0)
            {
                while (_delayTime > 0)
                {
                    yield return null;
                    Actor.UpdateLocomotion(Vector2.zero, Time.deltaTime);
                    _delayTime -= Time.deltaTime;
                }
            }
            //yield return IEPerform();
            _performRoutine = Actor.StartCoroutine(IEPerform());
        }

        void StopPerformRoutine()
        {
            if (_performRoutine != null)
            {
                Actor.StopCoroutine(_performRoutine);
            }

            if (_delayCoroutine != null)
            {
                Actor.StopCoroutine(_delayCoroutine);
            }
        }

        protected virtual IEnumerator IEPerform()
        {
            yield break;
        }

        public void Stop()
        {
            if (!Performing) return;
            Performing = false;
            ChargeCount = Mathf.Max(ChargeCount - 1, 0);
            IsCanMoveNextSkill = false;
            _delayTime = -1;
            StopPerformRoutine();
            Actor?.SetAnimatorSpeed();
            Actor?.SetRootMotionMult(1f);
            DoStop();
            TriggerChanged();
            Actor?.OnSkillStopped(this);
        }

        protected virtual void StopQuickActionSkill()
        {
            if (!Performing) return;
            Performing = false;
            ChargeCount = Mathf.Max(ChargeCount - 1, 0);
            StopPerformRoutine();
            DoStop();
            TriggerChanged();
            Actor?.OnSkillStopped(this);
        }

        protected virtual void DoStop()
        {
        }
        
        public virtual void OnBeginHit(int index)
        {
        }
        
        public virtual void OnStopIndicator(int index)
        {
        }
        
        public virtual void OnBeginMove(int index)
        {
        }

        public virtual void OnStopMove(int index)
        {
        }
        
        public virtual void OnBeginTracking(int index)
        {
        }

        public virtual void OnStopTracking(int index)
        {
        }

        public virtual void OnBeginTrail(int index)
        {
        }

        public virtual void OnStopTrail(int index)
        {
        }
        
        public virtual void OnPrepareShoot(int index)
        {
        }
        
        public virtual void OnChargeFull(int index)
        {
        }

        public virtual void OnShoot(int index)
        {
        }

        public virtual void OnAnimationMove(Vector3 deltaPosition)
        {
        }

        public virtual void OnPerformingUseSkill()
        {
        }

        public virtual void OnCustomEvent(int index)
        {
            if (skillPartialActions != null)
            {
                foreach (var action in skillPartialActions)
                {
                    if (action.Index == index && action.CanTrigger(Actor))
                    {
                        action.Trigger(this, Actor);
                    }
                }
            }
        }

        public virtual void OnFeedbackEvent(int index)
        {
        }
        
        public virtual void OnEndAttack()
        {
        }
        
        public void OnCanMoveNextSkill()
        {
            IsCanMoveNextSkill = true;
        }

        public Skill Get()
        {
            return this;
        }

        void TriggerChanged()
        {
            if (triggerChanged)
            {
                OnChangedEvent?.Invoke();
            }
        }

        public void CacheInput()
        {
            UseCachedInput = true;
            CachedAimedDirection = Actor.AimedDirection;
            CachedAimedPosition = Actor.GetAimPosition();
            CachedAimedEnemy = Actor.AimedEnemy;
        }
        
        protected void ResetCachedInput()
        {
            UseCachedInput = false;
        }

        public virtual void OnRollCancel()
        {
        }
        
        //Artifact info
        public int Power { get; private set; }
        public int Essence { get; private set; }
        public float BaseStat { get; private set; }
        public bool Upgraded { get; set; }

        public void SetPowerAndRank(int power, int essence, float baseStat)
        {
            Power = power;
            Essence = essence;
            BaseStat = baseStat;
        }

        public virtual string GetDesc()
        {
            return desc;
        } 
        
        //Hit Weapon Info
        protected int SpecialHitData { get; private set; }

        public void SetupUniqueHit(int index)
        {
            SpecialHitData = index;
        }

        public virtual void SetupAllyAimed(bool allyAimedValue)
        {
            
        }
    }

    public enum SkillType
    {
        None,
        KungfuBalance,
        KungfuCalm,
        KungfuWrath
    }
}