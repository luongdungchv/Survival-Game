using System.Collections;
using System.Collections.Generic;
using Enemy.Base;
using UnityEngine;

namespace Enemy.Low
{
    public class AtkSwitchBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("atk", false);
            Debug.Log("switch");
            var stats = animator.GetComponent<EnemyStats>();
            var distToTarget = stats.distanceToTarget;

            var inClose = distToTarget <= stats.closeRange - 0.5f;

            var rand = inClose ? 3 : Random.Range(0, 3);
            animator.SetInteger("atkIndex", rand);
        }
    }
}
