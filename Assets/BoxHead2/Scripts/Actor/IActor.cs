using System;
using System.Collections;
using System.Collections.Generic;
using BoxHead2.Collection;
using BoxHead2.Combat;
using BoxHead2.Items;
using BoxHead2.Skills;
using BoxHead2.SubActions;
using BoxHead2.SubCombat;
using UnityEngine;

namespace BoxHead2.Actor
{
    public interface IActor : IEquipmentAttachable
    {
        bool IsPreview { get; }
        bool Destroyed { get; }
        Vector3 Position { get; }
        Vector3 LockPosition { get; }
        Vector3 HeadPosition { get; }
        WeaponData Weapon { get; set; }
        WeaponData RangeWeapon { get; set; }
        float HpRatio { get; }
        float MaxHp { get; }
        float Heal(HealData healData);
        bool IsUnderStatusEffect(StatusEffectType statusEffect);
        Vector3 AimedDirection { get; }
        Vector3 GetAimPosition();
        bool DashConstant { get; set; }
        bool Alive { get; }
        public event System.Action<float, float> OnHpChanged;
        public event Action<IActor> OnDespawnEvent;
        void EnableMovementCollision();
        void DisableMovementCollision();
        float MovementCollisionRadius { get; }
        void ChangeMovementCollisionRadius(float radius);
        void TurnOffHitBox();
        void TurnOnHitBox();
        IDamageTaker AimedEnemy { get; }
        Vector3 ForwardDirection { get; }
        Vector3 GetMoveDirection();
        DamageTakerCollection AllyCollection { get; }
        DamageTakerCollection EnemyCollection { get; }
        IDamageTaker GetRandomEnemy(float range = -1, HashSet<IDamageTaker> except = null);
        float GetEnemyDistance();
        IDamageTaker GetNearestEnemy(float range, HashSet<IDamageTaker> except = null);
        bool IsEnemyInAttackRange(float offset = 0f);
        void Warp(Vector3 position);
        void MovePosition(Vector3 position, float speed, float rotateSpeed, float stoppingDistance);
        void StopMovement();
        void DisableMovement();
        void EnableMovement();
        void BeginShowWarning();
        void RotateDirection(Vector3 direction, float rotateSpeed, bool immediately = false);
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
        bool IsPlayingAnim { get; }
        bool IsInvisible { get; set; }
        void SetAnimatorSpeed(float speed = 1f);
        void OverrideAnimator(AnimatorOverrideController animatorOverrideController);
        float RunSpeed { get; }
        float RotateSpeed { get; }
        Animator Animator { get; }
        TeamConfig TeamConfig { get; }
        bool DealingDamage { get; set; }        
        void SetRootMotionMult(float mult);
        void PlayAnimation(string animName, int layer, float speed, bool fade, bool isLocomotion);
        void UpdateLocomotion(Vector2 vel);
        void UpdateLocomotion(Vector2 vel, float deltaTime);
        void StopCurrentSkill();
        void OnSkillBeginAttack();
        void OnSkillEndAttack();
        void OnSkillStopped(Skill skill);
        void OnSkillCanMoveNextSkill();
        void SpawnSkillText(string s, Vector3 position);
        List<IDamageTaker> GetEnemyInRadius(Vector3 position, float range);
        void MoveDirection(Vector3 direction, float speed);
        void OnRevive();
        void ClearNegativeStatusEffects();
        void ToIdleState();
        void ToDeadState(CombinedDamageForceData combinedDamageForceData);
        void InstantDead(CombinedDamageForceData combinedDamageForceData);
        void PlayTrailFx();
        void StopTrailFx();
        void PlayPassiveBuffFx(BuffType buffType);
        void StopPassiveBuffFx(BuffType buffType);
        void OnBeginRoll();
        void OnStopRoll();
        void SpawnStatusText(string text, Vector3 position);
        void Despawn(Vector3 position);
        event System.Action OnBuffChangedEvent;
        void AddBuff(BuffData buff);
        void RemoveBuff(BuffData buff);
        bool IsBuffActivated(BuffData.BuffType buffType);
        int RangedPerfectCount { get; set; }
        float LastTimeRangedPerfect { get; set; }
        public void ApplyMaterial(bool isInvisible);
        float MeleeAttackSpeedBonus { get; }
        bool IsPenetrateRanged { get; }
        void StartDash();
        void StopDash();
        void TurnOffVisual();
        void TurnOnVisual();
        void CheckOverrideAnimBySkin(string weaponId);

    }
}