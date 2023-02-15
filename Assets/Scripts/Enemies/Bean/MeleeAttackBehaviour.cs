using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class MeleeAttackBehaviour : StateMachineBehaviour
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
            
            navAgent.isStopped = true;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}
