using System;
using System.Collections.Generic;
using System.Linq;
using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.Combat
{
    [Serializable]
    public struct DamageConfig
    {
        [SerializeField] public DamageType damageType;
        [SerializeField] public DamageScaleType damageScaleType;
        [SerializeField] public bool canNotEvade;
        [SerializeField] public bool canNotFireStatusEvent;
        [SerializeField] public bool canOverrideDotDamageByWeapon;
        [SerializeField] public float damage;
        [SerializeField] public float damageBonus;
        [SerializeField] public float criticalChance;
        [SerializeField] public float staggerChance;
        [SerializeField] public float staggerDuration;
        [SerializeField] public float slowChance;
        [SerializeField] public float slowDuration;
        [SerializeField] public float slowValue;
        [SerializeField] public float backDamageBonus;
        [SerializeField] public float extendDamageDotDuration;
        [SerializeField] public int attackId;
        [SerializeField] public DamageForceData damageForceData;
        [SerializeField] public StatusEffectData[] StatusDamageEffectDatas;

        public DamageConfig(DamageConfig damageConfig)
        {
            damageType = damageConfig.damageType;
            damageScaleType = damageConfig.damageScaleType;
            canNotEvade = damageConfig.canNotEvade;
            canNotFireStatusEvent = damageConfig.canNotFireStatusEvent;
            canOverrideDotDamageByWeapon = damageConfig.canOverrideDotDamageByWeapon;
            damage = damageConfig.damage;
            damageBonus = damageConfig.damageBonus;
            criticalChance = damageConfig.criticalChance;
            staggerChance = damageConfig.staggerChance;
            staggerDuration = damageConfig.staggerDuration;
            slowChance = damageConfig.slowChance;
            slowDuration = damageConfig.slowDuration;
            slowValue = damageConfig.slowValue;
            backDamageBonus = damageConfig.backDamageBonus;
            extendDamageDotDuration = damageConfig.extendDamageDotDuration;
            attackId = damageConfig.attackId;
            damageForceData = damageConfig.damageForceData;
            StatusDamageEffectDatas = damageConfig.StatusDamageEffectDatas.ToArray();
        }
    }

    public struct DamageSourceData
    {
        public IDamageSource DamageSource;
        public float CriticalChanceBonus; // weapon critical + bonus
        public float CriticalDamageBonus; // weapon critical + bonus

        public float MissChance;

        public float DamageBonus; // bonus
        public float FinalDamageBonus; // bonus
        public float WeaponDamageBonus;
        public float FinalWeaponDamageBonus;

        public int AttackIndex;
        public float ChargeMultBonus;

        public float SlowDurationBonus;
        public float StaggerDurationBonus;

        public int SourceAttackId;
        public bool ForceCheckCritical;
        public float FinalDamageMultiplier;

        public float StaggerChanceBonus;

        public DamageSourceData(IDamageSource damageSource, float criticalChanceBonus, float criticalDamageBonus,
            float missChance,
            float damageBonus, float finalDamageBonus, float weaponDamageBonus, float finalWeaponDamageBonus, int attackIndex,
            float chargeMultBonus, float slowDurationBonus, float staggerDurationBonus, int sourceAttackId, bool forceCheckCritical, float finalDamageMultiplier, float staggerChanceBonus)
        {
            DamageSource = damageSource;
            CriticalChanceBonus = criticalChanceBonus;
            CriticalDamageBonus = criticalDamageBonus;
            MissChance = missChance;
            DamageBonus = damageBonus;
            FinalDamageBonus = finalDamageBonus;
            WeaponDamageBonus = weaponDamageBonus;
            AttackIndex = attackIndex;
            ChargeMultBonus = chargeMultBonus;
            SlowDurationBonus = slowDurationBonus;
            StaggerDurationBonus = staggerDurationBonus;
            SourceAttackId = sourceAttackId;
            ForceCheckCritical = forceCheckCritical;
            FinalWeaponDamageBonus = finalWeaponDamageBonus;
            FinalDamageMultiplier = finalDamageMultiplier;
            StaggerChanceBonus = staggerChanceBonus;    
        }
    }

    public struct CombinedDamageData
    {
        public IDamageSource DamageSource;
        public DamageType DamageType;
        public DamageScaleType DamageScaleType;
        public CombinedDamageForceData DamageForce;
        public bool InstantKill;
        public float DamageBonus;

        public float Damage;
        public float CriticalChance;
        public float CriticalDamage;
        public float MissChance;
        public float StaggerChance;
        public float StaggerDuration;
        public float SlowChance;
        public bool CanNotEvade;
        public bool CanNotFireStatusEvent;
        public int AttackIndexOfSource;
        public int AttackId;
        public float ChargeMultBonus;
        public float SlowDuration;
        public float BackDamageBonus;
        public float DamageCommitedBonus;
        public float SlowValue;
        public int SourceAttackId;
        public StatusEffectData[] StatusEffectDatas;
        public float ExtendDotDuration;
        public bool ForceCheckCritical;
        public float AdditionalFlatDamage;

        public CombinedDamageData(IDamageSource damageSource, DamageType damageType,
            DamageScaleType damageScaleType, CombinedDamageForceData damageForce, bool instantKill, float damage,
            float criticalChance, float criticalDamage, float missChance, float staggerChance, float staggerDuration,
            float slowChance, bool canNotEvade, bool canNotFireStatusEvent, int attackIndexOfSource, int attackId, float chargeMultBonus,
            float slowDuration, float backDamageBonus, float damageCommitedBonus, float slowValue, int sourceAttackId, StatusEffectData[] statusEffectDatas, float extendDotDuration, bool forceCheckCritical, float additionalFlatDamage)
        {
            DamageSource = damageSource;
            DamageType = damageType;
            DamageScaleType = damageScaleType;
            DamageForce = damageForce;
            InstantKill = instantKill;
            DamageBonus = 0;
            Damage = damage;
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            MissChance = missChance;
            StaggerChance = staggerChance;
            StaggerDuration = staggerDuration;
            SlowChance = slowChance;
            CanNotEvade = canNotEvade;
            CanNotFireStatusEvent = canNotFireStatusEvent;
            AttackIndexOfSource = attackIndexOfSource;
            AttackId = attackId;
            ChargeMultBonus = chargeMultBonus;
            SlowDuration = slowDuration;
            BackDamageBonus = backDamageBonus;
            DamageCommitedBonus = damageCommitedBonus;
            SlowValue = slowValue;
            SourceAttackId = sourceAttackId;
            StatusEffectDatas = statusEffectDatas;
            ExtendDotDuration = extendDotDuration;
            ForceCheckCritical = forceCheckCritical;
            AdditionalFlatDamage = additionalFlatDamage;
        }

        public static CombinedDamageData Combine(DamageSourceData sourceData, DamageConfig damageConfig)
        {
            if (sourceData.DamageSource != null)
            {
                damageConfig = sourceData.DamageSource.BuffModifyDamageConfig(damageConfig);
            }

            var damage = damageConfig.damage;
            var criticalChance = sourceData.CriticalChanceBonus + damageConfig.criticalChance;
            var criticalDamage = sourceData.CriticalDamageBonus;
            var damageCommitedBonus = 0f;

            var isOffHandWeapon = damageConfig.damageType.HasFlag(DamageType.OffHand);
            var isWeaponDamageDeal = damageConfig.damageType.HasFlag(DamageType.Weapon);
            // damage scale
            var weaponDamage = isOffHandWeapon
                ? sourceData.DamageSource?.OffHandWeaponDamage ?? 0
                : sourceData.DamageSource?.WeaponDamage ?? 0;
            
            switch (damageConfig.damageScaleType)
            {
                case DamageScaleType.Raw:
                    break;
                case DamageScaleType.InheritWeaponDamage:
                    if (sourceData.DamageSource == null) break;
                    damage *= weaponDamage;
                    if (isWeaponDamageDeal)
                    {
                        criticalChance += isOffHandWeapon
                            ? sourceData.DamageSource.OffHandWeaponCriticalChance
                            : sourceData.DamageSource.WeaponCriticalChance;
                        criticalDamage += isOffHandWeapon
                            ? sourceData.DamageSource.OffHandWeaponCriticalDamage
                            : sourceData.DamageSource.WeaponCriticalDamage;
                        damageCommitedBonus += isOffHandWeapon
                            ? sourceData.DamageSource.OffHandWeaponCommittedBonus
                            : sourceData.DamageSource.WeaponCommittedBonus;
                    }

                    break;
                case DamageScaleType.ScaleByEnemyMaxHp:
                    break;
                case DamageScaleType.ScaleByEnemyHp:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // damage bonus
            var damageBonus = sourceData.DamageBonus + damageConfig.damageBonus;
            var finalWeaponDamageBonus = 0f;
            if (damageConfig.damageType.HasFlag(DamageType.Weapon) && damageConfig.damageType.HasFlag(DamageType.Melee))
            {
                damageBonus += sourceData.WeaponDamageBonus;
                finalWeaponDamageBonus += sourceData.FinalWeaponDamageBonus;
            }

            damage *= Mathf.Max(0.1f, 1 + damageBonus);
            damage *= 1 + sourceData.FinalDamageBonus + finalWeaponDamageBonus;

            damage *= (1 + sourceData.FinalDamageMultiplier);
            var staggerDuration = damageConfig.staggerDuration * (1 + sourceData.StaggerDurationBonus);
            var damageForce = new CombinedDamageForceData(damageConfig.damageForceData.type,
                damageConfig.damageForceData.duration, damageConfig.damageForceData.force,
                damageConfig.damageForceData.reverse, !damageConfig.damageForceData.preventPullEscape,
                damageConfig.damageForceData.overrideCurrentDamageForce, damageConfig.damageForceData.addToCurrentDamageForce,
                Vector3.zero, Vector3.zero, Vector3.zero);

            var slowDuration = damageConfig.slowDuration * (1 + sourceData.SlowDurationBonus);
            var slowValue = damageConfig.slowValue;
            var statusEffects = new List<StatusEffectData>();

            var staggerChance = damageConfig.staggerChance + sourceData.StaggerChanceBonus;
            
            if (damageConfig.StatusDamageEffectDatas is { Length: > 0 } && sourceData.DamageSource is Actor.Actor actor)
            {
                foreach (var statusEffect in damageConfig.StatusDamageEffectDatas)
                {
                    var statusEffectData = new StatusEffectData(statusEffect);
                    switch (statusEffectData.type)
                    {
                        case StatusEffectType.None:
                            break;
                        case StatusEffectType.Slow:
                            break;
                        case StatusEffectType.Stagger:
                            break;
                        case StatusEffectType.Poison:
                            if (damageConfig.canOverrideDotDamageByWeapon)
                            {
                                statusEffectData.damageValue *= weaponDamage;
                            }
                            statusEffectData.duration *= (1 + actor.PoisonDurationBonus);
                            statusEffectData.duration += actor.PoisonDurationBonusFlat;
                            statusEffectData.damageValue *= (1 + actor.BonusDamageDot + actor.PoisonDamageBonus);
                            statusEffectData.stackIncrease = Mathf.Max(1, statusEffect.stackIncrease) + actor.PoisonStackBonus + actor.DotStackBonus;
                            break;
                        case StatusEffectType.Bleed:
                            statusEffectData.duration *= (1 + actor.BleedDurationBonus);
                            statusEffectData.duration += actor.BleedDurationBonusFlat;
                            statusEffectData.damageValue *= (1 + actor.BonusDamageDot + actor.BleedDamageBonus);
                            statusEffectData.stackIncrease = Mathf.Max(1, statusEffect.stackIncrease) + actor.BleedStackBonus + actor.DotStackBonus;
                            break;
                        case StatusEffectType.Burn:
                            if (damageConfig.canOverrideDotDamageByWeapon)
                            {
                                statusEffectData.damageValue *= weaponDamage;
                            }
                            statusEffectData.duration *= (1 + actor.BurnDurationBonus);
                            statusEffectData.duration += actor.BurnDurationBonusFlat;
                            statusEffectData.damageValue *= (1 + actor.BonusDamageDot + actor.BurnDamageBonus);
                            statusEffectData.stackIncrease = Mathf.Max(1, statusEffect.stackIncrease) + actor.BurnStackBonus + actor.DotStackBonus;
                            statusEffectData.dotEffectData.burnCanSpread = actor.DealBurnSpread;
                            break;
                        case StatusEffectType.Cinch:
                            break;
                        case StatusEffectType.Blind:
                            break;
                        default:
                            break;
                    }
                    statusEffects.Add(statusEffectData);
                }
            }
            
            if (sourceData.DamageSource != null)
            {
                var statusBonus = sourceData.DamageSource.GetStatusEffectsBuffData(damageConfig);
                if (statusBonus is { Length: > 0 })
                {
                    statusEffects.AddRange(statusBonus);
                }
            }

            var additionalFlatDamage = 0f;

            return new CombinedDamageData(
                sourceData.DamageSource,
                damageConfig.damageType,
                damageConfig.damageScaleType,
                damageForce,
                false,
                damage,
                criticalChance,
                criticalDamage,
                sourceData.MissChance,
                staggerChance,
                staggerDuration,
                damageConfig.slowChance,
                damageConfig.canNotEvade,
                damageConfig.canNotFireStatusEvent,
                sourceData.AttackIndex,
                damageConfig.attackId,
                sourceData.ChargeMultBonus,
                slowDuration,
                damageConfig.backDamageBonus,
                damageCommitedBonus,
                slowValue,
                sourceData.SourceAttackId,
                statusEffects.ToArray(),
                damageConfig.extendDamageDotDuration,
                sourceData.ForceCheckCritical,
                additionalFlatDamage
            );
        }

        public void UpdateDamageForce(Vector3 dir, Vector3 pos, Vector3 centerPosition)
        {
            DamageForce.HitDirection = dir;
            DamageForce.HitPosition = pos;
            DamageForce.CenterPosition = centerPosition;
        }

        public void OverrideDamageForce(DamageForceData damageForce)
        {
            DamageForce.Type = damageForce.type;
            DamageForce.Duration = damageForce.duration;
            DamageForce.Force = damageForce.force;
            DamageForce.Reverse = damageForce.reverse;
            DamageForce.OverrideCurrentDamageForce = damageForce.overrideCurrentDamageForce;
        }
    }

    public struct FinalDamageData
    {
        public IDamageSource DamageSource;
        public IDamageTaker DamageTarget;
        public DamageType DamageType;
        public CombinedDamageForceData DamageForce;
        public bool InstantKill;
        public bool IsCriticalHit;
        public int Damage;
        public StatusEffect[] StatusEffects;
        public bool FireStatusEffectEvent;
        public int AttackIndex;
        public int AttackId;
        public float ChargeMultBonus;
        public float ExtendDotDuration;

        public FinalDamageData(IDamageSource damageSource, IDamageTaker damageTarget, DamageType damageType,
            CombinedDamageForceData damageForce, bool instantKill, bool isCriticalHit, int damage,
            StatusEffect[] statusEffects, bool fireStatusEffectEvent, int attackIndex, int attackId,
            float chargeMultBonus, float extendDotDuration)
        {
            DamageSource = damageSource;
            DamageTarget = damageTarget;
            DamageType = damageType;
            DamageForce = damageForce;
            InstantKill = instantKill;
            IsCriticalHit = isCriticalHit;
            Damage = damage;
            StatusEffects = statusEffects;
            FireStatusEffectEvent = fireStatusEffectEvent;
            AttackIndex = attackIndex;
            AttackId = attackId;
            ChargeMultBonus = chargeMultBonus;
            ExtendDotDuration = extendDotDuration;
        }
    }

    [Serializable]
    public struct DamageForceData
    {
        [SerializeField] public DamageForceType type;
        [SerializeField] public float duration;
        [SerializeField] public float force;
        [SerializeField] public bool reverse;
        [SerializeField] public bool preventPullEscape;
        [SerializeField] public bool overrideCurrentDamageForce;
        [SerializeField] public bool addToCurrentDamageForce;
    }

    [Serializable]
    public struct StatusEffectData
    {
        [SerializeField] public StatusEffectType type;
        [SerializeField] public float chance;
        [SerializeField] public float duration;
        [SerializeField] public float tickInterval;
        [SerializeField] public float damageValue;
        [SerializeField] public int stackIncrease;
        [SerializeField] public DOTEffectData dotEffectData; 

        public StatusEffectData(StatusEffectData data)
        {
            type = data.type;
            chance = data.chance;
            duration = data.duration;
            tickInterval = data.tickInterval;
            damageValue = data.damageValue;
            stackIncrease = data.stackIncrease;
            dotEffectData = new DOTEffectData(data.dotEffectData);
        }
    }

    [Serializable]
    public struct DOTEffectData
    {
        [SerializeField] public float poisonDeBuffDamage;
        [SerializeField] public bool burnCanSpread;
        [SerializeField] public float bleedIncreaseDamageTaken;
        [SerializeField] public float burnIncreaseDamageTaken;
        [SerializeField] public float bleedHealForSource;

        public DOTEffectData(DOTEffectData data)
        {
            poisonDeBuffDamage = data.poisonDeBuffDamage;
            burnCanSpread = data.burnCanSpread;
            bleedIncreaseDamageTaken = data.bleedIncreaseDamageTaken;
            burnIncreaseDamageTaken = data.burnIncreaseDamageTaken;
            bleedHealForSource = data.bleedHealForSource;
        }
    }

    public struct CombinedDamageForceData
    {
        public DamageForceType Type;
        public float Duration;
        public float Force;
        public bool Reverse;
        public bool CanPullEscape;
        public bool OverrideCurrentDamageForce;
        public bool AddToCurrentDamageForce;
        public Vector3 CenterPosition;
        public Vector3 HitPosition;
        public Vector3 HitDirection;

        public CombinedDamageForceData(DamageForceType type, float duration, float force, bool reverse,
            bool canPullEscape,
            bool overrideCurrentDamageForce, bool addToCurrentDamageForce, Vector3 centerPosition, Vector3 hitPosition, Vector3 hitDirection)
        {
            Type = type;
            Duration = duration;
            Force = force;
            Reverse = reverse;
            CanPullEscape = canPullEscape;
            OverrideCurrentDamageForce = overrideCurrentDamageForce;
            CenterPosition = centerPosition;
            HitPosition = hitPosition;
            HitDirection = hitDirection;
            AddToCurrentDamageForce = addToCurrentDamageForce;
        }
    }

    public enum DamageForceType
    {
        None,
        KnockBack,
        Knockdown,
        Pull,
    }

    [Flags]
    public enum DamageType
    {
        None = 0,
        Melee = 1,
        Ranged = 2,
        Weapon = 4,
        Skill = 8,
        Explode = 16,
        Pet = 32,
        OffHand = 64
    }

    public enum DamageScaleType
    {
        InheritWeaponDamage,
        Raw,
        ScaleByEnemyMaxHp,
        ScaleByEnemyHp
    }
}