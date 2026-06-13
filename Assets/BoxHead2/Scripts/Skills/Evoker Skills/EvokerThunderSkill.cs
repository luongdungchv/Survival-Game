using BoxHead2.Actor;
using BoxHead2.Combat;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Skills
{
    public class EvokerThunderSkill : CoordinateAttackSkill
    {
        [SerializeField] int attackCount;
        [SerializeField] float indicatorDuration;
        [SerializeField] float atkRadius;
        [SerializeField] string atkLoopAnim, atkEndAnim;
        protected override int AttackCount => attackCount;
        
        Vector3 playerPos => NetworkPlayer.localPlayer.transform.position;
        
        protected override void Attack(int index)
        {
            if (_activeIndicators != null && index < _activeIndicators.Length)
            {
                var indicator = _activeIndicators[index];
                if (indicator)
                {
                    pools.Despawn(indicator);
                }
            }

            var attackItem = pools.Spawn(attackItemPrefab);

            var spawnPoint = GetAttackPosition(index);
            attackItem.transform.position = spawnPoint;
            attackItem.SetDamageData(_combinedDamageData);
            attackItem.SetDamageLayer(Actor.TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer));
            attackItem.StartDealingDamage();
        }

        protected override void SpawnIndicator(int index)
        {
            if (!indicatorPrefab) return;
            var indicator = pools.Spawn(indicatorPrefab);
            _activeIndicators[index] = indicator;
            indicator.GetComponent<MaterialModifyIndicator>().StartUpdate(indicatorDuration);
        }

        protected override void OnAttackAnimStop()
        {
            weapon.StopCharge();
            Actor.PlayAnimation(atkEndAnim, 0, 1, false, false);
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            if (index == 0)
                weapon.StartCharge();
            else if (index == 1)
                Actor.PlayAnimation(atkLoopAnim, 0, 1, false, false);
        }

        protected override Vector3 GetAttackPosition(int index)
        {
            return playerPos;
        }
    }
}