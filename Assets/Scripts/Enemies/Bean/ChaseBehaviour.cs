using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class ChaseBehaviour : StateMachineBehaviour
    {
        private Transform target;
        private IChase chaseStats;
        private NavMeshAgent navAgent;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var stats = animator.GetComponent<EnemyStats>();
            navAgent = animator.GetComponent<NavMeshAgent>();
            target = stats.target;
            chaseStats = stats as IChase;
            navAgent.isStopped = false;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navAgent.destination = target.position;
            // var distance = Vector3.Distance(animator.transform.position, target.position);
            // if(distance > chaseStats.chaseRange){
            //     animator.SetTrigger("patrol");
            // }
        }
    }
}
