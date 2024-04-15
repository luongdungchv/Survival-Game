using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private EnemyHitbox hitbox;
    private void Start() {
        hitbox = GetComponentInChildren<EnemyHitbox>();
    }
    public void Attack(){
        hitbox.DetectHit();
    }
}
