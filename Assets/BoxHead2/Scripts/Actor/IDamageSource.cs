using System.Collections.Generic;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Actor
{
    public interface IDamageSource
    {
        public ActorType Type { get; }
        public TeamConfig TeamConfig { get; }
        public bool Alive { get; }
        public int WeaponDamage { get; }
        public float WeaponCriticalChance { get; }
        public float WeaponCriticalDamage { get; }
        public float WeaponCommittedBonus { get; }
        public int OffHandWeaponDamage { get; }
        public float OffHandWeaponCriticalChance { get; }
        public float OffHandWeaponCriticalDamage { get; }
        public float OffHandWeaponCommittedBonus { get; }
        public float BonusDamageDot { get; }
        public float MaxHp { get; }
        public Vector3 Position { get; }
        public Vector3 AimedDirection { get; }
        float Heal(HealData healData);
        public void OnDealDamageToEnemy(DamageType damageType, int attackIndex, int attackId, float damage, float chageMultBonus, bool canLifeSteal, IDamageTaker damageTaker);
        public DamageSourceData GetDamageSourceData();
        public void OnCriticalDamage();
        public IDamageTaker GetRandomEnemy(float range = -1, HashSet<IDamageTaker> except = null);
        public DamageConfig BuffModifyDamageConfig(DamageConfig damageConfig);
        public StatusEffectData[] GetStatusEffectsBuffData(DamageConfig damageConfig);
    }
}