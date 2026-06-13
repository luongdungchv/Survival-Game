using System.Collections;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class DealDamageAroundSkill : Skill
    {
        [SerializeField] string animation;
        [SerializeField] float indicatorDuration;
        [SerializeField] CoordinateAttackItem atkItemPrefab;
        [SerializeField] AttachConfig attachConfig;
        [SerializeField] DamageConfig damageConfig;
        [SerializeField] ParticleSystem[] feedbackFXPrefabs;
        [SerializeField] MaterialModifyIndicator indicatorPrefab;
        [SerializeField] Feedback[] feedbacks;
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        
        CombinedDamageData _combinedDamageData;
        Weapon _weapon;

        MaterialModifyIndicator _activeIndicator;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _weapon = SkillHelper.GetAttachedEquipments<Weapon>(Actor, new[] { attachConfig })[0];
            _combinedDamageData = CombinedDamageData.Combine(GetDamageSourceData(),damageConfig);
        }

        protected override IEnumerator IEPerform()
        {
            Actor.PlayAnimation(animation, 0, 1, false, false);
            while (true)
            {
                yield return null;
            }
        }

        public override void OnBeginHit(int index)
        {
            base.OnBeginHit(index);
            if (!indicatorPrefab) return;
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
            _activeIndicator = pools.Spawn(indicatorPrefab);
            _activeIndicator.transform.position = Actor.Position + Vector3.up * 0.1f;
            _activeIndicator.transform.localScale = Vector3.one * atkItemPrefab.Radius;
            _activeIndicator.StartUpdate(indicatorDuration);

        }

        public override void OnStopIndicator(int index)
        {
            base.OnStopIndicator(index);
            if (!indicatorPrefab) return;
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
        }

        public override void OnFeedbackEvent(int index)
        {
            base.OnFeedbackEvent(index);
            if (feedbacks.Length > 0)
            {
                var fbIndex = Mathf.Clamp(index, 0, feedbacks.Length - 1);
                feedbacks[fbIndex]?.Play();
            }

            if (feedbackFXPrefabs.Length > 0)
            {
                var fbIndex = Mathf.Clamp(index, 0, feedbackFXPrefabs.Length - 1);
                var prefab = feedbackFXPrefabs[fbIndex];
                if (!prefab) return;
                var fx = pools.Spawn(prefab);
                fx.transform.position = Actor.Position + prefab.transform.localPosition;
                fx.Play();
            }
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            _weapon.StartCharge();
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            _weapon.StopCharge();
            var attackItem = pools.Spawn(atkItemPrefab);
            attackItem.transform.position = Actor.Position + Vector3.up * 0.12f;
            attackItem.SetDamageData(_combinedDamageData);
            attackItem.SetDamageLayer(Actor.TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer));
            attackItem.StartDealingDamage();
        }

        protected override void DoStop()
        {
            base.DoStop();
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
        }
    }
}