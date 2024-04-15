using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Low
{
    public class DashAtkBehaviour : StateMachineBehaviour
    {
        private float elapsed;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed = 0;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= stateInfo.length)
            {

                animator.Play("Attack Switch");

            }
        }
    }
}
