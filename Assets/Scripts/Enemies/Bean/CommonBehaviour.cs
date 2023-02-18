using System.Collections;
using System.Collections.Generic;
using Enemy.Base;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class CommonBehaviour : StateMachineBehaviour
    {
        // Start is called before the first frame update
        private Dictionary<string, NetworkPlayer> playerList;
        private Transform target;
        private IPatrol patrolStats;
        private EnemyStats stats;
        private IChase chaseStats;
        private NavMeshAgent navAgent;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //playerList = NetworkManager.ins.GetAllPlayers();
            stats = animator.GetComponent<EnemyStats>();
            target = stats.target;
            patrolStats = animator.GetComponent<IPatrol>();
            chaseStats = animator.GetComponent<IChase>();
            navAgent = animator.GetComponent<NavMeshAgent>();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!Client.ins.isHost) return;
            if (NetworkManager.ins == null) return;
            if (target == null) target = SelectTarget(animator.transform.position);
            stats.target = target;
            if (target == null) return;

            try
            {
                if(navAgent.navMeshOwner == null) return;
                var distToTarget = Vector3.Distance(target.position, animator.transform.position);
                if (distToTarget <= stats.atkRange)
                {
                    navAgent.isStopped = true;
                    Debug.Log(stats.target);
                    SetTrigger(animator, "atk");
                }
                else if (distToTarget <= chaseStats.chaseRange && distToTarget > stats.atkRange)
                    SetTrigger(animator, "chase");
                else if ((stateInfo.IsName("Chase") || stateInfo.IsName("Attack")) && distToTarget > chaseStats.chaseRange) SetTrigger(animator, "patrol");
            }
            catch
            {

            }
        }
        private void SetTrigger(Animator animator, string trigger)
        {
            animator.ResetTrigger("atk");
            animator.ResetTrigger("idle");
            animator.ResetTrigger("chase");
            animator.ResetTrigger("patrol");
            animator.SetTrigger(trigger);
        }
        private Transform SelectTarget(Vector3 position)
        {
            var playerList = NetworkManager.ins.GetAllPlayers();
            foreach (var i in playerList)
            {
                if (i.Value.GetComponent<PlayerStats>().isDead) continue;
                var distance = Vector3.Distance(position, i.Value.transform.position);
                if (distance <= chaseStats.chaseRange) return i.Value.transform;
            }
            return null;
        }
    }
}
