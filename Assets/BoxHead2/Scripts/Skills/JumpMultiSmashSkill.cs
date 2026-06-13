using System.Collections;
using System.Collections.Generic;
using BoxHead2.Combat;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class JumpMultiSmashSkill : JumpSmashSkill
    {
        [SerializeField] float smashDelay;
        
        Coroutine _smashCoroutine;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            if (_smashCoroutine != null) Actor.StopCoroutine(_smashCoroutine);
        }

        public override void OnShoot(int index)
        {
            DespawnIndicators();
            _smashCoroutine = Actor.StartCoroutine(IEDealDamage());
        }

        protected override void DoStop()
        {
            base.DoStop();
            if(_smashCoroutine != null) Actor.StopCoroutine(_smashCoroutine);
        }

        IEnumerator IEDealDamage()
        {
            for (int i = 0; i < _hurtBoxes.Length; i++)
            {
                _hurtBoxGroup.DamageTakers.Clear();
                var hurtBox = _hurtBoxes[i];
                var hits = hurtBox.Scan(Actor.TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer));
                foreach (var hit in hits)
                {
                    var position = hurtBox.transform.position;
                    var direction = hurtBox.explosionForce ? hit.DamageTaker.Position - position : hurtBox.transform.forward;
                    direction.y = 0;
                    direction.Normalize();
                    _combinedDamageData.UpdateDamageForce(direction, hit.HitPosition, position);
                    hit.DamageTaker.TakeDamage(_combinedDamageData);
                }

                PlaySmashFX(hurtBox.transform.position + Vector3.one * 0.1f, hurtBox.radius);
                yield return new WaitForSeconds(smashDelay);
            }
            DespawnHurtBoxes();
        }

        public override void OnBeginMove(int index)
        {
            base.OnBeginMove(index);
            
            var targetPos = GetAimedPosition();
            var aimDir = targetPos - Actor.Position;
            aimDir.y = 0;
            aimDir.Normalize();
            
            _hurtBoxGroup.transform.rotation = Quaternion.FromToRotation(Vector3.forward, aimDir);
        }

        protected override void ShowIndicator()
        {
            foreach (var hurtBox in _hurtBoxes)
            {
                var indicator = pools.Spawn(indicatorPrefab);
                activeIndicators.Add(indicator);
                indicator.transform.position = hurtBox.transform.position;
                indicator.transform.localScale = Vector3.one.Set(y : 0) * hurtBox.radius + Vector3.up;
                indicator.StartUpdate(indicatorDuration);
            }
        }

    }
}