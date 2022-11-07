using Enemy.Base;
using UnityEngine;

namespace Enemy.Low
{
    public class BodyHurtbox : MonoBehaviour, IDamagable
    {
        [SerializeField] private EnemyStats stats;
        public void OnDamage(IHitData hitData)
        {
            var playerHitData = hitData as PlayerHitData;
            stats.hp -= playerHitData.damage;
            if (stats.hp <= 0) Destroy(stats.gameObject);
        }
    }
}