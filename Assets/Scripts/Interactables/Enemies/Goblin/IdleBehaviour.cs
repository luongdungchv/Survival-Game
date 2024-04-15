using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Low
{
    public class IdleBehaviour : StateMachineBehaviour
    {
        private float elapsed;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed = 0;
            var rb = animator.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= 3)
            {
                animator.SetTrigger("patrol");
            }
        }

    }

}