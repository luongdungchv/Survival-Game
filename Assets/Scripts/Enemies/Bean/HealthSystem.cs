using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class HealthSystem : MonoBehaviour, IDamagable
    {
        [SerializeField] private float maxHP;
        [SerializeField] private float currentHP;
        [SerializeField] private float knockbackStrength = 40;
        [SerializeField] private int minCoinsDrop, maxCoinsDrop;
        private NavMeshAgent navAgent;
        private void Start() {
            currentHP = maxHP;
            navAgent = GetComponent<NavMeshAgent>();
        }
        public void OnDamage(IHitData hitData)
        {
            var playerHitData = hitData as PlayerHitData;
            currentHP -= playerHitData.damage;
            
            if(currentHP <= 0){
                Destroy(this.gameObject);
                var coins = Random.Range(minCoinsDrop, maxCoinsDrop + 1);
                playerHitData.dealer.coins += coins;
                return;
            }
            if(playerHitData.knockback){
                var dealerPos = playerHitData.dealer.pivot.position;
                dealerPos.y = 0;
                var position = transform.position;
                position.y = 0;
                var knockbackDir = position - dealerPos;
                navAgent.isStopped = false;
                //StartCoroutine(Knockback(knockbackDir, 0.05f));
                navAgent.velocity = knockbackDir.normalized * knockbackStrength;
                StartCoroutine(KnockbackCD(0.2f));
            }
        }
        private IEnumerator KnockbackCD(float duration){
            yield return new WaitForSeconds(duration);
            navAgent.velocity = Vector3.zero;
        }
    }
}
