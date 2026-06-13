using System.Collections;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using UnityEngine;

namespace BoxHead2.Skills
{
    public abstract class CoordinateAttackSkill : Skill
    {
        [SerializeField] bool useCoroutineForAttack;
        [SerializeField] bool useCoroutineForIndicator;
        [SerializeField] string animation;
        [SerializeField] protected AttachConfig attachConfig;
        [SerializeField] DamageConfig damageConfig;
        [SerializeField] protected GameObject indicatorPrefab;
        [SerializeField] protected CoordinateAttackItem attackItemPrefab;
        float attackInterval;
        float indicatorInterval;
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        protected abstract int AttackCount { get; }

        protected Vector3[] _attackPostitions;
        protected GameObject[] _activeIndicators;

        Coroutine attackCoroutine, indicatorCoroutine;
        protected CombinedDamageData _combinedDamageData;

        protected RangedWeapon weapon;
        
        protected abstract void Attack(int index);
        protected abstract void SpawnIndicator(int index);
        protected abstract void OnAttackAnimStop();
        protected virtual Vector3 GetAttackPosition(int index) => GetAimedPosition();

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _attackPostitions ??= new Vector3[AttackCount];
            _activeIndicators ??= new GameObject[AttackCount];
            _combinedDamageData = CombinedDamageData.Combine(GetDamageSourceData(), damageConfig);
            weapon = SkillHelper.GetAttachedEquipments<Weapon>(Actor, new[] { attachConfig })[0] as RangedWeapon;
        }

        protected override IEnumerator IEPerform()
        {
            OnStartPerform();
            while (true)
            {
                OnPerforming();
                yield return null;
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if (useCoroutineForAttack && index == 0)
            {
                attackCoroutine = Actor.StartCoroutine(IEAttack());
            }
            else
            {
                Attack(index);
            }
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            if (useCoroutineForIndicator && index == 0)
            {
                indicatorCoroutine = Actor.StartCoroutine(IESpawnIndicator());
            }
            else
            {
                SpawnIndicator(index);
            }
        }

        protected override void DoStop()
        {
            base.DoStop();
            if (attackCoroutine != null)
                Actor.StopCoroutine(attackCoroutine);
            if (indicatorCoroutine != null)
                Actor.StopCoroutine(indicatorCoroutine);
            foreach (var indicator in _activeIndicators)
            {
                if(!indicator) continue;
                pools.Despawn(indicator);
            }
        }
        
        protected virtual void OnStartPerform()
        {
            Actor.PlayAnimation(animation, 0, 1, false, false);
        }

        protected virtual void OnPerforming()
        {
            
        }

        IEnumerator IEAttack()
        {
            var wait = new WaitForSeconds(attackInterval);
            for (int i = 0; i < AttackCount; i++)
            {
                Attack(i);
                yield return wait;
            }

            OnAttackAnimStop();
        }

        IEnumerator IESpawnIndicator()
        {
            var wait = new WaitForSeconds(indicatorInterval);
            for (int i = 0; i < AttackCount; i++)
            {
                SpawnIndicator(i);
                yield return wait;
            }
        }
    }
}