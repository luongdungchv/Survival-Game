using Enemy.Base;
using UnityEngine;

namespace Enemy.Low
{
    public class BodyHurtbox : DamagableObject
    {
        [SerializeField] private EnemyStats stats;
        public override void OnDamage(IHitData hitData)
        {
            var playerHitData = hitData as PlayerHitData;
            stats.hp -= playerHitData.damage;
            if (stats.hp <= 0) Destroy(stats.gameObject);
        }
    }
}