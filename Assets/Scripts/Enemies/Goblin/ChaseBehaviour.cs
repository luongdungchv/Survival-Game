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

            //target = Vector3.zero;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // var dirToTarget = target.position - animator.transform.position;
            // var disttoTarget = dirToTarget.magnitude;

            // animator.transform.LookAt(target.position);
            // var rb = animator.GetComponent<Rigidbody>();
            // rb.velocity = dirToTarget.normalized * (stats as IMove).runSpeed;
            navmesh.destination = target.position;
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navmesh.isStopped = true;
            navmesh.velocity = Vector3.zero;
        }
    }

}