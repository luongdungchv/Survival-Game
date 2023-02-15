using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class PatrolWalkBehaviour : StateMachineBehaviour
    {
        private Transform target;
        private IPatrol patrolStats;
        private NavMeshAgent navAgent;
        private EnemyStats stats;
        private Vector3 targetPos;
        private LayerMask mask;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stats = animator.GetComponent<EnemyStats>();
            navAgent = animator.GetComponent<NavMeshAgent>();
            target = stats.target;
            patrolStats = stats as IPatrol;
            navAgent.isStopped = false;
            
            var patrolCenter = patrolStats.patrolCenter;
            var patrolArea = patrolStats.patrolArea;
                 
            var randX = Random.Range(patrolCenter.x - patrolArea.x, patrolCenter.x + patrolArea.x);
            var randY = Random.Range(patrolCenter.z - patrolArea.y, patrolCenter.z + patrolArea.y);
            targetPos = new Vector3(randX, animator.transform.position.y, randY);
            var dirToTarget = targetPos - animator.transform.position;
            var disttoTarget = dirToTarget.magnitude;
            
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navAgent.destination = targetPos;
            var disttoTarget = Vector3.Distance(targetPos, animator.transform.position);
            if (disttoTarget <= 0.5f)
            {
                animator.ResetTrigger("patrol");
                animator.SetTrigger("idle");
            }
        }
    }
}
