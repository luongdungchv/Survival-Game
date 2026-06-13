using BoxHead2.Combat;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Actor
{
    public interface IDamageTaker
    {
        public ActorType Type { get; }
        public bool CanBeTarget { get; }
        public Transform Transform { get; }
        public Vector3 LockPosition { get; }
        public Vector3 Position { get; }
        public bool Destroyed { get; }
        public bool Alive { get; }
        public bool IsStaggering { get; }
        public bool IsCinch { get; }
        public bool Initialized { get; set; }
        public HitInfo TakeDamage(CombinedDamageData combinedDamageData);
        public void InstantDead(CombinedDamageForceData damageForceData);
        public void ApplyTrueDamage(float damage, ActorType actorType);
        public float Heal(HealData healData);
        public event System.Action<float, float> OnHpChanged;
        public event System.Action OnStatusEffectChanged;
        public event System.Action<StatusEffectType, int> OnStatusEffectDmgTaken;
        public void WakeUpAttack();
    }
}