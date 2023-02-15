using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class IdleBehaviour : StateMachineBehaviour
    {
        // Start is called before the first frame update
        private float elapsed;
        private NavMeshAgent navAgent;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed = 0;
            navAgent = animator.GetComponent<NavMeshAgent>();
            navAgent.isStopped = true;
            //animator.SetTrigger("patrol");
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= 3)
            {
                animator.ResetTrigger("idle");
                animator.SetTrigger("patrol");
            }
            //navAgent.destination = Vector3.up * -250.44f;
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }
    }
}
