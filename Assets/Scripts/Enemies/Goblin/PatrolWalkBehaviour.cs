using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;

namespace Enemy.Low
{
    public class PatrolWalkBehaviour : StateMachineBehaviour
    {
        private Vector3 target;
        private IPatrol stats;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stats = animator.GetComponent<IPatrol>();
            var patrolCenter = stats.patrolCenter;
            var patrolArea = stats.patrolArea;

            var randX = Random.Range(patrolCenter.x - patrolArea.x, patrolCenter.x + patrolArea.x);
            var randY = Random.Range(patrolCenter.z - patrolArea.y, patrolCenter.z + patrolArea.y);
            target = animator.transform.right * randX + animator.transform.forward * randY;
            //target = Vector3.zero;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var dirToTarget = target - animator.transform.position;
            var disttoTarget = dirToTarget.magnitude;
            if (disttoTarget <= 0.5f)
            {
                animator.SetTrigger("idle");
                //return;
            }
            var rb = animator.GetComponent<Rigidbody>();
            rb.velocity = dirToTarget.normalized;
        }
    }

}