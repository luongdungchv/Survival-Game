using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;

namespace Enemy.Low
{
    public class PlayerDetector : StateMachineBehaviour
    {
        private Transform player;
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            player = animator.GetComponent<EnemyStats>().target;
        }
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            player = animator.GetComponent<EnemyStats>().target;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }

}