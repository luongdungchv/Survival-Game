using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Enemy.Base;

namespace Enemy.Low
{
    public class WalkAroundBehaviour : StateMachineBehaviour
    {
        private Transform target;
        private EnemyStats stats;
        private Rigidbody rb;
        private float elapsed, duration;
        public int left;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stats = animator.GetComponent<EnemyStats>();
            target = stats.target;
            rb = animator.GetComponent<Rigidbody>();
            elapsed = 0;
            duration = Random.Range(0.7f, 1.7f);
            Debug.Log("walk around");

        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= duration)
            {
                animator.Play("Attack Switch");
            }
            var dirToTarget = target.position - animator.transform.position;
            var groundNormal = (stats as IGround).groundNormal;
            var moveDir = Vector3.Cross(dirToTarget, groundNormal).normalized;
            rb.velocity = moveDir * left * (stats as IMove).walkSpeed;
            animator.transform.LookAt(target.position);
        }
        private void PlayRandom(Animator animator, string[] states)
        {
            int index = Random.Range(0, states.Length);
            animator.Play(states[index]);
        }
    }

}