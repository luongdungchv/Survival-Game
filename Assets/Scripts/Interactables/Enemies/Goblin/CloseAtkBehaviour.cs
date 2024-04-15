using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;

namespace Enemy.Low
{
    public class CloseAtkBehaviour : StateMachineBehaviour
    {
        private Transform target;
        private EnemyStats stats;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stats = animator.GetComponent<EnemyStats>();
            target = animator.transform;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var dist = stats.distanceToTarget;
            if (dist > stats.closeRange + 0.3f && dist < stats.atkRange - 0.7f)
            {
                animator.Play("Attack Switch");
            }
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger("atkIndex", -1);
        }
    }

}