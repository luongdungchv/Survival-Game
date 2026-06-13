using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoxHead2.Collection;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using BoxHead2.Skills;
using BoxHead2.SubActions;
using Dacodelaac.Events;
using Dacodelaac.FiniteStateMachine;
using Dacodelaac.Utils;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoxHead2.Actor
{
    public abstract class AIEnemy : Actor, IAIEnemy
    {
        [Header("Enemy")]
        [SerializeField] ParticleSystem buffActiveFxPrefab;
        [SerializeField] ParticleSystem buffFx;
        [SerializeField] protected AICollection aiCollection;
        [SerializeField] Vector3Event lastHitEvent;
        [SerializeField] Feedback lastHitFeedback;

        public AIConfig AIConfig { get; private set; }
        public ExtraType ExtraType => AIConfig.extraType;
        public EnemyStat EnemyStat { get; set; }
        public virtual bool CanDrop { get; set; }
        public virtual bool CanDropFlower => AIConfig?.canDropFlower ?? false;
        public int ZoneIndex { get; set; }

        protected virtual Vector3 DropPosition => LockPosition;
        //public DropConfig[] DropConfigs { get; set; }
        //public int UraniumDrop { get; set; }

        public bool IsPatrolBlockedByTut => false;
        public bool IsEliteOrBoss => Config.type.HasFlag(ActorType.Elite) || Config.type.HasFlag(ActorType.Boss);
        public virtual bool IsAttackTimeMatch => CanPerformAction && Time.time - LastTimeAttack > AttackInterval;
        public virtual float EnemyCloseRange => Config.enemyCloseRange;
        public override bool CanPerformAction => !IsStaggering && !IsPulling && !IsKnockingDown 
                                                && (!Config.canGetHitAnim || StateMachine.CurrentState is not GetHitBaseState || IsEliteOrBoss || AIConfig.canAttackWhenGetHit);

        protected virtual float AttackInterval => AIConfig.attackInterval;
        

        public override bool CanGetHitAnim => (!Attacking || (AIConfig.stopAttackWhenHit > 0 && Random.value <= AIConfig.stopAttackWhenHit)) && base.CanGetHitAnim;
        public SkillCombo CurrentSkillCombo { get; set; }
        Dictionary<SpecialSkillId, List<SpecialSkill>> specialSkills;
        SkillCombo[] skillCombos;
        protected int Phase;
        
        //Stats
        public override int WeaponDamage => EnemyStat.Damage;
        public override float WeaponCriticalChance => EnemyStat.CriticalChance;
        public override float WeaponCriticalDamage => EnemyStat.CriticalDamage;
        public override float CriticalChanceBonus => 0;
        public override float CriticalDamageBonus => 0;
        public override float DamageBonus => enemyBuff.DamageBonus;
        public override float FinalDamageBonus => -PoisonDeBuffDamage;
        public override float FinalDamageTakenBonus => BleedIncreaseTakenDamage + BurnIncreaseTakenDamage + _deBuffEnemyTakenDamage.DamageTakenBonus;
        public override float DamageReduction => EnemyStat.DamageReduction;
        public override float Evade => 0;
        public override float DamageTaken => 0;
        public override float MaxHp => EnemyStat.MaxHp * (1 + enemyBuff.HpBonus);
        public override float MoveSpeedBonus => enemyBuff.MovementSpeedBonus + (EnemyStat.IsMutant ? 0.5f : 0f);
        public override float RunSpeed => Config.runSpeed * (1 + MoveSpeedBonus) * SlowFactor * (IsCinch ? 0 : 1);
        public override bool IgnoreDamage => IsIgnoreDamage || base.IgnoreDamage;
        public virtual float WalkSpeed => Config.walkSpeed * SlowFactor * (IsCinch ? 0 : 1);

        //
        public override Vector3 AimedDirection => aimedDirection;
        public virtual bool CanBeHostile => false;

        public SkillCombo[] SkillCombos => skillCombos;
        
        List<IDamageTaker> crazyAllySet; 
        protected List<Renderer> renderersBound = new();

        Vector3 aimedDirection;
        EnemyBuff enemyBuff;
        DeBuffEnemyTakenDamage _deBuffEnemyTakenDamage;

        protected Bounds bounds;
        public static bool IsIgnoreDamage = false;
        public float LastTimePatrol { get; set; }
        public Vector3 DefaultPos { get; set; }
        public bool IsStartAttack { get; set; }
        bool _isAwakeAlly;
        IDamageTaker _bindEnemyTarget;
        bool _forceTargetToBindEnemy;
        
        public override void BindVariable()
        {
            base.BindVariable();
            aiCollection.Add(this);
        }

        public override void UnbindVariable()
        {
            base.UnbindVariable();
            if (aiCollection.Contains(this))
            {
                aiCollection.Remove(this);
            }
        }

        protected void BindVariableGrand()
        {
            base.BindVariable();
        }

        protected void UnbindVariableGrand()
        {
            base.UnbindVariable();
        }

        public override void SetConfig(ActorConfig config, TeamConfig teamConfig)
        {
            AIConfig = config as AIConfig;
            base.SetConfig(config, teamConfig);
        }

        protected override void InitProperties()
        {
            Phase = 0;
            LastTimePatrol = 0;
            UpdateDefaultPosition();
            base.InitProperties();
            LastTimeAttack = Time.time - AttackInterval + AIConfig?.firstAttackDelay ?? 0;
            CurrentSkillCombo = null;
            AimedEnemy = null;
            crazyAllySet = new List<IDamageTaker>();
            IsStartAttack = false;
            _isAwakeAlly = false;
            SetupIfMutant();
            ClearGetHit();
            _bindEnemyTarget = null;
            _forceTargetToBindEnemy = false;
        }
        

        public void UpdateDefaultPosition()
        {
            DefaultPos = Position;
        }
        
        public virtual Bounds CalculateBounds()
        {
            if (renderersBound.Count == 0)
            {
                renderersBound.AddRange(GetComponentsInChildren<MeshRenderer>());
                renderersBound.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
            }

            bounds = new Bounds();
            for (var index = 0; index < renderersBound.Count; index++)
            {
                var r = renderersBound[index];

                if (index == 0)
                {
                    bounds = r.bounds;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            return bounds;
        }
        
        public virtual void OnPrespawn()
        {
        }

        protected override void InitSkills()
        {
            base.InitSkills();
            
            specialSkills = AIConfig.CloneSpecialSkills(this);
            skillCombos = AIConfig.CloneSkillCombo(this, false);
        }
        
        public bool IsEnemyClose(bool checkSkillRange = true)
        {
            Skill skill = null;
            if(checkSkillRange) skill = GetCurrentOrNextSkill();
            var enemyCloseRange = checkSkillRange ? 
                (skill == null ? EnemyCloseRange : Mathf.Min(EnemyCloseRange, skill.Range)) : EnemyCloseRange;
            return AimedEnemy != null && SimpleMath.InRange(Position, AimedEnemy.Position, enemyCloseRange);
        }

        public void PrepareCurrentComboSkill(bool renew, string skillName = "")
        {
            if (!string.IsNullOrEmpty(skillName))
            {
                var skillCombo = skillCombos.FirstOrDefault(sc => sc.skillName == skillName);
                if (skillCombo != null)
                {
                    CurrentSkillCombo = skillCombo;
                    CurrentSkillCombo.IsForcePlayWhenConditionMatch = false;
                    return;
                }
            }
            if (CurrentSkillCombo != null && !renew)
            {
                return;
            }

            var availableSkills = skillCombos.Where(sc => sc.Condition.IsMatchCondition(this) && sc.IsForcePlayWhenConditionMatch).ToArray();
            if (availableSkills.Length == 0)
            {
                availableSkills = skillCombos.Where(sc => sc.Condition.IsMatchCondition(this) && sc.Available).ToArray();
            }
            if (availableSkills.Length > 0)
            {
                CurrentSkillCombo = SimpleMath.GetWeightedRandomItems(availableSkills,
                    availableSkills.Select(a => a.Weight).ToArray(), 1, false)[0];
                CurrentSkillCombo.IsForcePlayWhenConditionMatch = false;
            }
        }

        public override void Tick()
        {
            if (!Initialized) return;
            if (AIConfig && GetNearestEnemy(AIConfig.aimToEnemyRange) == null)
            {
                StopMovement();
                UpdateStatus();
                UpdateLocomotion(Vector2.zero, Time.deltaTime);
                return;
            }
            base.Tick();

            if (Time.time - LastTimeTakeDamage > 5f)
            {
                if (_bindEnemyTarget is { Alive: true })
                {
                    _forceTargetToBindEnemy = true;
                }
            }
            
            for (var i = 0; i < skillCombos.Length; i++)
            {
                skillCombos[i].UpdateCooldown(Time.deltaTime);
            }
            for (var i = crazyAllySet.Count - 1; i >= 0; i--)
            {
                if (!crazyAllySet[i].Alive)
                {
                    crazyAllySet.RemoveAt(i);
                }
            }
        }

        protected override void UpdateStatusEffect()
        {
            base.UpdateStatusEffect();
            UpdateEnemyBuff();
        }
        
        void UpdateEnemyBuff()
        {
            if (enemyBuff.IsValid)
            {
                enemyBuff.Tick(Time.deltaTime);
            }
            if (enemyBuff.IsValid)
            {
                if (buffFx && buffFx.isStopped) buffFx.Play();
            }
            else
            {
                if (buffFx && buffFx.isPlaying) buffFx.Stop();
            }

            if (_deBuffEnemyTakenDamage.IsValid)
            {
                _deBuffEnemyTakenDamage.Tick(Time.deltaTime);
            }
        }

        public void Buff(EnemyBuff buff)
        {
            var hpAdd = buff.HpBonus * MaxHp;
            enemyBuff = buff;
            Hp = Mathf.Min(MaxHp, Hp + hpAdd);
        }

        public void HealMaxHp()
        {
            Hp = MaxHp;
        }

        public void DeBuffDamageTaken(DeBuffEnemyTakenDamage deBuff)
        {
            _deBuffEnemyTakenDamage = deBuff;
        }

        protected override void ClearStatusEffects()
        {
            base.ClearStatusEffects();
            enemyBuff = new EnemyBuff();
            _deBuffEnemyTakenDamage = new DeBuffEnemyTakenDamage();
            ResetFx(buffFx);
        }

        public bool TryGetSpecialSkill(SpecialSkillId id, out List<Skill> list)
        {
            list = new List<Skill>();
            if (specialSkills.TryGetValue(id, out var sks))
            {
                list = sks.Where(l => l.Condition.IsMatchCondition(this)).Select(s => s.Skill).ToList();
            }
            return list.Count > 0;
        }

        protected override Skill GetNextWeaponSkill()
        {
            PrepareCurrentComboSkill(false);
            if (CurrentSkillCombo != null && CurrentSkillCombo.Skills != null && CurrentSkillCombo.Skills.Length > 0)
            {
                return CurrentSkillCombo.Skills[0];
            }

            return null;
        }

        protected override void HandleDamageForce(DamageType damageType, CombinedDamageForceData damageForce)
        {
            //BlinkGetHit();
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
                        if (Alive && !IsAttackTimeMatch)
                        {
                            StateMachine.ChangeState<GetHitBaseState>(damageForce);
                        }
                    }
                    break;
                case DamageForceType.Pull:
                    _pullHp -= DamageForce.Duration;
                    DamageForce = damageForce;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected override IDamageTaker GetAimedEnemy(Vector3 forward, float range, bool followAngle = false)
        {
            return GetNearestEnemy(range);
        }

        protected override void UpdateDirection()
        {
            aimedDirection = Transform.forward;
            AimedEnemy = GetAimedEnemy(aimedDirection, AIConfig ? (!IsStartAttack ? AIConfig.startAttackRange : AIConfig.aimToEnemyRange) : 15f);
            
            if (AimedEnemy != null)
            {
                IsStartAttack = true;
                var dir = AimedEnemy.LockPosition - Position;
                //dir.y = 0;
                dir.Normalize();
                if (dir != Vector3.zero)
                {
                    aimedDirection = dir;
                }
            }
        }

        public override void WakeUpAttack()
        {
            IsStartAttack = true;
        }

        public bool IsReachDestination()
        {
            return ActorMovement.IsReachDestination();
        }

        public virtual float GetCustomSkillCondition(int index)
        {
            return -1;
        }
        
        public int GetPhase()
        {
            return Phase;
        }


        public virtual void SetPhase(int phase)
        {
            Phase = phase;
        }

        public override void OnGetHitEnded()
        {
            if (!IsEliteOrBoss)
            {
                base.OnGetHitEnded();
            }
        }
        
        public void ReduceAttackCooldown()
        {
            LastTimeAttack -= AttackInterval * 1f;
        }

        protected override void ExtendLastTimeAttack()
        {
            if (LastTimeAttack + AttackInterval - Time.time < 1f)
            {
                LastTimeAttack = Time.time + 1f - AttackInterval;
            }
        }

        protected virtual void CheckLastHit()
        {
           
        }

        public void DelayTurnOnHitBox(float time)
        {
            this.Delay(time, false, () => TurnOnHitBox());
        }

        public override void ToDeadState(CombinedDamageForceData damageForceData)
        {
            CheckLastHit();
            DOTween.Kill(this);
            base.ToDeadState(damageForceData);
        }


        public void StopPreview()
        {
            StopCurrentSkill();
            Hp = 0f;
            Despawn(Vector3.zero);
        }

        public virtual void OnBossDie()
        {
            InstantDead(new CombinedDamageForceData());
        }

        public virtual void TriggerAttack(string skillName = "")
        {
            PrepareCurrentComboSkill(false, skillName); 
            StateMachine.ChangeState<AIBaseAttackState>();
        }

        protected override StatusEffect EvaluateStatusEffect(StatusEffect statusEffect)
        {
            if (IsEliteOrBoss)
            {
                switch (statusEffect.Type)
                {
                    case StatusEffectType.None:
                        break;
                    case StatusEffectType.Slow:
                        statusEffect.Value = 0;
                        break;
                    case StatusEffectType.Stagger:
                        if (Time.time - LastTimeFreezeOrStaggerOrFear > 10f || EnemyStat.IsMutant)
                        {
                            statusEffect.TickInterval /= 2;
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    default:
                        break;
                }
            }

            return statusEffect;
        }
        protected override HitInfo TakeDamageInternal(CombinedDamageData combinedDamage)
        {
            if (combinedDamage.DamageSource is Actor)
            {
                _forceTargetToBindEnemy = false;
                
                if (!IsStartAttack)
                {
                    IsStartAttack = true;
                }

                if (!_isAwakeAlly)
                {
                    _isAwakeAlly = true;
                    foreach (var ally in AllyCollection)
                    {
                        if (SimpleMath.InRange(Position, ally.Position, AIConfig.startAttackRange, true) && ally.Alive)
                        {
                            ally.WakeUpAttack();
                        }
                    }
                }
            }

            if (combinedDamage.DamageSource is AIEnemy aiEnemy && aiEnemy != this && aiEnemy.Alive && aiEnemy.CanBeHostile
                && AllyCollection.Contains(aiEnemy) && !crazyAllySet.Contains(aiEnemy))
            {
                crazyAllySet.Add(aiEnemy);
            }
            return base.TakeDamageInternal(combinedDamage);
        }
        public override IDamageTaker GetNearestEnemy(float range, HashSet<IDamageTaker> except = null)
        {
            var player = NetworkPlayer.localPlayer;
            if (!player) return null;
            if (Vector2.Distance(player.transform.position, Position) > range) return null;
            return NetworkPlayer.localPlayer.GetComponent<IDamageTaker>();
        }
        
        

        public virtual void OnRetreat()
        {
        }
        
        protected virtual void SetupIfMutant()
        {
            if (EnemyStat.IsMutant)
            {
                //Mutant();
            }
        }

        protected override void SpawnHealText(float amount)
        {
        }

        public override void CleanUp()
        {
            base.CleanUp();
            if (skillCombos != null)
            {
                foreach (var skill in skillCombos)
                {
                    skill.Detach();
                }
                skillCombos = Array.Empty<SkillCombo>();
            }
            if (specialSkills != null)
            {
                foreach (var skills in specialSkills.Values)
                {
                    foreach (var skill in skills)
                    {
                        skill.Detach();
                    }
                }
                specialSkills.Clear();
            }
        }

        protected override void HandleDie(CombinedDamageForceData damageForceData)
        {
            base.HandleDie(damageForceData);
        }
        
        void BlinkGetHit()
        {
            if (renderers.Length <= 0) return;
            //if (Config.type.HasFlag(ActorType.Boss)) return;
            StartCoroutine(SetBlinkGetHit());
        }

        IEnumerator SetBlinkGetHit()
        {
            float time = 0;
            while (time < 2)
            {
                var value = time > 1 ? 1 - time % 1 : time % 1;
                foreach (var t in renderers)
                {
                    t.material.SetFloat("_GetHitEmissionRate", value);
                }
                time += Time.deltaTime / 0.35f;
                yield return null;
            }
            foreach (var t in renderers)
            {
                t.material.SetFloat("_GetHitEmissionRate", 0);
            }
        }

        void ClearGetHit()
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_GetHitEmissionRate", 0);
            }
        }

        [Sirenix.OdinInspector.Button]
        void LogTest()
        {
            Debug.LogError((this.StateMachine.CurrentState, Animator.GetCurrentAnimatorStateInfo(0).IsName("locomotion"), IsPlayingAnim));
        }
        
    }
    
    public class EnemyStat
    {
        public int Damage;
        public float CriticalChance;
        public float CriticalDamage;
        public float DamageReduction;
        public float MaxHp;
        public bool IsMutant;
        public int Difficulty;
        public double Exp;
        
        public EnemyStat(float criticalChance, float criticalDamage, int damage,
            float damageReduction, float maxHp, bool isMutant, double exp)
        {

            Damage = damage;
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            DamageReduction = damageReduction;
            MaxHp = maxHp;
            IsMutant = isMutant;
            Exp = exp;
            if (IsMutant)
            {
                DamageReduction += 0.1f;
                MaxHp *= 2;
                Damage = Mathf.RoundToInt(Damage * 3f);
            }
        }

        public EnemyStat(EnemyStat otherStat)
        {
            Damage = otherStat.Damage;
            CriticalChance = otherStat.CriticalChance;
            CriticalDamage = otherStat.CriticalDamage;
            DamageReduction = otherStat.DamageReduction;
            MaxHp = otherStat.MaxHp;
            IsMutant = otherStat.IsMutant;
            Difficulty = otherStat.Difficulty;
            Exp = otherStat.Exp;
            if (IsMutant)
            {
                DamageReduction += 0.1f;
                MaxHp *= 2;
                Damage = Mathf.RoundToInt(Damage * 3f);
            }
        }
    }
}