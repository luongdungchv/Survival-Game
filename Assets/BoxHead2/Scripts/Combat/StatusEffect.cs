using System;
using BoxHead2.Actor;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Combat
{
    public class StatusEffect
    {
        public StatusEffectType Type { get; set; }
        public IDamageSource DamageSource { get; set; }
        public int SourceId { get; set; }
        public int TickCount { get; set; }
        public int Stack { get; set; }
        public int RemainTick { get; set; }
        public float TickInterval { get; set; }
        public float CurrentTickTime { get; set; }
        public float RemainTimeRatio { get; set; }
        public DOTEffectData DotEffectData { get; set; }
        public bool IsExpired => RemainTick <= 0;
        public float Value { get; set; }
        public float BleedIncreaseTakenDamage => DotEffectData.bleedIncreaseDamageTaken;
        public float BurnIncreaseTakenDamage => DotEffectData.burnIncreaseDamageTaken;
        public float PoisonDeBuffDamage => DotEffectData.poisonDeBuffDamage;
        public float BleedHealForSource => DotEffectData.bleedHealForSource;
        
        public const int TickSlow = 2;
        public const int TickStagger = 2;
        public const int TickCinch = 2;
        public const int TickBlind = 2;
        public const float BurnSpreadRange = 2f;
        
        //Stack
        const int StackLimit = 20;
        //Poison
        const float PoisonMult = 1.1f;
        const float PoisonPow = 0.15f;
        const float PoisonMaxInternalTickInterval = 0.3f;

        //Burn
        const float BurnMult = 1.685f;
        const float BurnPow = 0.1f;
        const float BurnMaxInternalTickInterval = 0.5f;

        //Bleed
        const float BleedMult = 1f;
        const float BleedPow = 0.2f;
        const float BleedMaxInternalTickInterval = 1f;
        
        public StatusEffect(StatusEffectType type, int tick, float interval, float value, int stack, DOTEffectData dotEffectData, int sourceId, IDamageSource source)
        {
            Type = type;
            switch (Type)
            {
                case StatusEffectType.Poison:
                    interval = Mathf.Min(PoisonMaxInternalTickInterval);
                    break;
                case StatusEffectType.Bleed:
                    interval = Mathf.Min(BleedMaxInternalTickInterval);
                    break;
                case StatusEffectType.Burn:
                    interval = Mathf.Min(BurnMaxInternalTickInterval);
                    break;
            }

            CurrentTickTime = TickInterval = interval;
            TickCount = RemainTick = tick;
            Value = value;
            RemainTimeRatio = 1;
            SourceId = sourceId;
            Stack = Mathf.Max(1, stack);
            DotEffectData = dotEffectData;
            DamageSource = source;
        }

        public void IncreaseStack(int bonus)
        {
            Stack += bonus;
            Stack = Mathf.Min(StackLimit, Stack);
        }

        public void Tick(Actor.Actor actor, float deltaTime)
        {
            if (!actor.Alive || IsExpired) return;

            CurrentTickTime += deltaTime;
            
            if (CurrentTickTime >= TickInterval)
            {
                RemainTick--;
                CurrentTickTime = 0;
                switch (Type)
                {
                    case StatusEffectType.Slow:
                        break;
                    case StatusEffectType.Stagger:
                        break;
                    case StatusEffectType.Cinch:
                        break;
                    case StatusEffectType.Blind:
                        break;
                    case StatusEffectType.Poison:
                        var poisonDamage = Value * (1 + PoisonMult * (1 - Mathf.Exp(-PoisonPow * (Stack - 1))));
                        actor.TakeStatusEffectDamage(Type, poisonDamage);
                        break;
                    case StatusEffectType.Bleed:
                        var bleedValue = Value * (1 + BleedMult * (1 - Mathf.Exp(-BleedPow * (Stack - 1))));
                        if (actor.Alive && DamageSource is {Alive : true} && BleedHealForSource > 0)
                        {
                            DamageSource.Heal(new HealData
                            {
                                Amount = actor.Hp * bleedValue * BleedHealForSource,
                                ActiveFx = true,
                                HpText = false,
                                Revive = false,
                                ReviveText = false,
                            });
                        }
                        actor.TakeStatusEffectDamage(Type, bleedValue);
                        break;
                    case StatusEffectType.Burn:
                        var burnDamage = Value * (1 + BurnMult * (1 - Mathf.Exp(-BurnPow * (Stack - 1))));
                        actor.TakeStatusEffectDamage(Type, burnDamage);
                        break;
                    default:
                        break;
                }
            }

            RemainTimeRatio = (float)(RemainTick - 1) / TickCount + Mathf.Clamp01(1 - CurrentTickTime / TickInterval);
        }

        public void ResetCooldown()
        {
            RemainTick = TickCount - 1;
            RemainTimeRatio = (float)(RemainTick - 1) / TickCount + Mathf.Clamp01(1 - CurrentTickTime / TickInterval);
        }

        public void ExtendDuration(float duration)
        {
            if (IsExpired) return;
            var addedTick = Mathf.RoundToInt(duration / TickInterval);
            RemainTick += addedTick;
            TickCount += addedTick;
        }
    }

    [Flags]
    public enum StatusEffectType
    {
        None = 0,
        Slow = 1,
        Stagger = 2,
        Poison = 4,
        Bleed = 8,
        Burn = 16,
        Cinch = 32,
        Blind = 64,
    }
}