using System;
using BoxHead2.Combat;
using UnityEngine;

namespace BoxHead2.Actor
{
    public interface IAIEnemy
    {
        public ActorType Type { get; }
        public ExtraType ExtraType { get; }
        public bool IsDying { get; }
        public bool Alive { get; }
        public void SetPhase(int phase);
        public void Buff(EnemyBuff buff);
        public void DeBuffDamageTaken(DeBuffEnemyTakenDamage deBuff);
        public bool IgnoreDamage { get; }
        public void Despawn(Vector3 position);
        public void StopPreview();
        public void InstantDead(CombinedDamageForceData damageForceData);
        public HitInfo TakeDamage(CombinedDamageData combinedDamageData);
        public void OnBossDie();
    }
    
    [Serializable]
    public struct EnemyBuff
    {
        [SerializeField] float damageBonus;
        [SerializeField] float movementSpeedBonus;
        [SerializeField] float hpBonus;
        [SerializeField] float duration;

        public bool IsValid => duration > 0;

        public float DamageBonus => IsValid ? damageBonus : 0;
        public float MovementSpeedBonus => IsValid ? movementSpeedBonus : 0;
        public float HpBonus => IsValid ? hpBonus : 0;

        public EnemyBuff CreateCopy()
        {
            var enemyBuff = new EnemyBuff
            {
                duration = duration,
                movementSpeedBonus = movementSpeedBonus,
                hpBonus = hpBonus,
                damageBonus = damageBonus
            };
            return enemyBuff;
        }

        public void Tick(float deltaTime)
        {
            duration -= deltaTime;
        }
    }
    
    [Serializable]
    public struct DeBuffEnemyTakenDamage
    {
        [SerializeField] float damageTakenBonus;
        [SerializeField] float duration;

        public bool IsValid => duration > 0;

        public float DamageTakenBonus => IsValid ? damageTakenBonus : 0; 
        public DeBuffEnemyTakenDamage CreateCopy()
        {
            var enemyBuff = new DeBuffEnemyTakenDamage
            {
                duration = duration,
                damageTakenBonus = damageTakenBonus
            };
            return enemyBuff;
        }

        public DeBuffEnemyTakenDamage(float damageTakenBonus, float duration)
        {
            this.damageTakenBonus = damageTakenBonus;
            this.duration = duration;
        }

        public void Tick(float deltaTime)
        {
            duration -= deltaTime;
        }
    }
}