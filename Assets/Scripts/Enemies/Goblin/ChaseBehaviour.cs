using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using UnityEngine.AI;

namespace Enemy.Low
{
    public class ChaseBehaviour : StateMachineBehaviour
    {
        private Transform target;
        private EnemyStats stats;
        private NavMeshAgent navmesh;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stats = animator.GetComponent<EnemyStats>();
            target = stats.target;
            animator.SetBool("chase", false);
            navmesh = animator.GetComponent<NavMeshAgent>();
            navmesh.isStopped = false;

        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navmesh.destination = target.position;
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navmesh.isStopped = true;
            navmesh.velocity = Vector3.zero;
        }
    }

}