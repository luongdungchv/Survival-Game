using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

namespace BoxHead2.Skills
{
    public class JumpSmashSkill : Skill
    {

        [SerializeField, ShowIf("noRootMotion")] float jumpHeight;
        [SerializeField] string animation;
        [SerializeField] float moveDuration;
        [SerializeField] bool noRootMotion;
        [SerializeField] HurtBoxGroup hurtboxGroupPrefab;
        [SerializeField] protected MaterialModifyIndicator indicatorPrefab;
        [SerializeField] AttachConfig[] attachConfigs;
        [SerializeField] DamageConfig damageConfig;
        [SerializeField] ParticleSystem fxSmashPrefab;
        [SerializeField] ParticleSystem[] otherFeedbackPrefabs;
        [SerializeField] Feedback feedbackSmash;
        [SerializeField] protected float indicatorDuration;
        [SerializeField] float jumpOffset;
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        
        protected HurtBoxGroup _hurtBoxGroup;
        protected HurtBox[] _hurtBoxes;

        protected CombinedDamageData _combinedDamageData;

        protected List<MaterialModifyIndicator> activeIndicators;

        Weapon[] _weapons;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            activeIndicators ??= new List<MaterialModifyIndicator>();
            activeIndicators.Clear();
            _combinedDamageData = CombinedDamageData.Combine(GetDamageSourceData(), damageConfig);
            _weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
        }

        protected override void DoStop()
        {
            base.DoStop();
            DespawnHurtBoxes();
            DespawnIndicators();
        }

        protected override IEnumerator IEPerform()
        {
            Actor.PlayAnimation(animation, 0, 1, false, false);
            while (true)
            {
                OnCustomSkillUpdate();
                yield return null;
            }
        }

        protected virtual void OnCustomSkillUpdate()
        {
            
        }

        public override void OnBeginMove(int index)
        {
            base.OnBeginMove(index);
            
            var targetPos = GetAimedPosition();
            if (NavMesh.SamplePosition(targetPos, out var hit, 10, NavMesh.AllAreas))
                targetPos = hit.position;
            var aimDir = targetPos - Actor.Position;
            aimDir.y = 0;
            aimDir.Normalize();
            targetPos += -aimDir * jumpOffset;

            SetupHurtBoxes();

            _hurtBoxGroup.transform.position = targetPos;
            
            var distance = Vector3.Distance(targetPos, Actor.Position);
            Actor.MovePosition(targetPos, distance / moveDuration * 1.3f, 999, 0.2f);
            Actor.Animator.transform.DOLocalJump(Vector3.zero, jumpHeight, 1, moveDuration);
        }

        public override void OnBeginHit(int index)
        {
            base.OnBeginHit(index);
            ShowIndicator();
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            _weapons[index].StartCharge();
        }
        public override void OnStopTrail(int index)
        {
            base.OnStopTrail(index);
            _weapons[index].StopCharge();
        }

        protected void PlaySmashFX(Vector3 position, float size = 1)
        {
            var fxSmash = pools.Spawn(fxSmashPrefab);
            fxSmash.transform.position = position;
            fxSmash.Play();
            fxSmash.transform.localScale = Vector3.one * size;
            feedbackSmash?.Play();
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            _hurtBoxGroup.DamageTakers.Clear();
            DespawnIndicators();
            foreach (var hurtBox in _hurtBoxes)
            {
                var hits = hurtBox.Scan(Actor.TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer));
                foreach (var hit in hits)
                {
                    var position = hurtBox.transform.position;
                    var direction = hurtBox.explosionForce
                        ? hit.DamageTaker.Position - position
                        : hurtBox.transform.forward;
                    direction.y = 0;
                    direction.Normalize();
                    _combinedDamageData.UpdateDamageForce(direction, hit.HitPosition, position);
                    hit.DamageTaker.TakeDamage(_combinedDamageData);
                }
            }
        }

        protected void SetupHurtBoxes()
        {
            _hurtBoxGroup = pools.Spawn(hurtboxGroupPrefab);
            _hurtBoxes = new HurtBox[_hurtBoxGroup.transform.childCount];
            for (var i = 0; i < _hurtBoxGroup.transform.childCount; i++)
            {
                _hurtBoxes[i] = _hurtBoxGroup.transform.GetChild(i).GetComponent<HurtBox>();
            }
            _hurtBoxGroup.Initialize();
        }

        protected void DespawnHurtBoxes()
        {
            if (_hurtBoxes == null || _hurtBoxGroup == null) return;
            pools.Despawn(_hurtBoxGroup.gameObject);
            _hurtBoxes = null;
            _hurtBoxGroup = null;
        }

        protected virtual void ShowIndicator()
        {
            var indicator = pools.Spawn(indicatorPrefab);
            indicator.transform.position = hurtboxGroupPrefab.transform.position;
            activeIndicators.Add(indicator);
            indicator.StartUpdate(indicatorDuration);
        }

        protected void DespawnIndicators()
        {
            if (activeIndicators == null) return;
            foreach (var indicator in activeIndicators)
            {
                if (indicator == null) continue;
                pools.Despawn(indicator.gameObject);
            }
        }
    }
}