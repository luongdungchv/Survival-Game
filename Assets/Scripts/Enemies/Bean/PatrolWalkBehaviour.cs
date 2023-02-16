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
        [SerializeField] private Vector3 targetPos;
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
                 
            mask = LayerMask.GetMask("Terrain", "Water");
            var targetSelected = SelectTarget(animator, ref targetPos);
            while(!targetSelected) targetSelected = SelectTarget(animator, ref targetPos);
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navAgent.destination = targetPos;
            var dirToTarget = targetPos - animator.transform.position;
            dirToTarget.y = 0;
            var disttoTarget = dirToTarget.magnitude;
            if (disttoTarget <= 0.4f)
            {
                animator.ResetTrigger("patrol");
                animator.SetTrigger("idle");
            }
        }
        private bool SelectTarget(Animator animator,ref Vector3 target){
            var patrolCenter = patrolStats.patrolCenter;
            var patrolArea = patrolStats.patrolArea;
            
            var randX = Random.Range(patrolCenter.x - patrolArea.x, patrolCenter.x + patrolArea.x);
            var randY = Random.Range(patrolCenter.z - patrolArea.y, patrolCenter.z + patrolArea.y);
            var castPos = new Vector3(randX, 100, randY);
            // targetPos = new Vector3(randX, animator.transform.position.y, randY);
            // var dirToTarget = targetPos - animator.transform.position;
            // var disttoTarget = dirToTarget.magnitude;
            if(Physics.Raycast(castPos, Vector3.down, out var hit, 110, mask)){
                if(hit.collider.tag == "Water") return false;
                target = hit.point;
                return true;
            }
            return false;
        }
    }
}
