using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Bean
{
    public class HealthSystem : MonoBehaviour, IDamagable
    {
        [SerializeField] private float maxHP;
        [SerializeField] private float currentHP;
        private void Start() {
            currentHP = maxHP;
        }
        public void OnDamage(IHitData hitData)
        {
            var playerHitData = hitData as PlayerHitData;
            currentHP -= playerHitData.damage;
            if(currentHP <= 0){
                Destroy(this.gameObject);
            }
        }
    }
}
