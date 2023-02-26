﻿using System.Collections;
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
        [SerializeField] private bool stopped;
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

            var netObj = animator.GetComponent<NetworkSceneObject>();
            if (Client.ins.isHost)
            {
                var targetSelected = SelectTarget(animator, ref targetPos);
                while (!targetSelected) targetSelected = SelectTarget(animator, ref targetPos);
                var updatePacket = new RawActionPacket(PacketType.UpdateEnemy)
                {
                    playerId = "0",
                    objId = netObj.id,
                    action = "patrol",
                    actionParams = new string[]{
                        targetPos.x.ToString(),
                        targetPos.y.ToString(),
                        targetPos.z.ToString(),
                    }
                };
                Client.ins.SendTCPPacket(updatePacket);
            }
            else{
                targetPos = stats.targetPos;
            }
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navAgent.destination = targetPos;
            var dirToTarget = targetPos - animator.transform.position;
            dirToTarget.y = 0;
            stopped = navAgent.isStopped;
            var disttoTarget = dirToTarget.magnitude;
            if (disttoTarget <= 0.4f)
            {
                animator.ResetTrigger("patrol");
                animator.SetTrigger("idle");
            }
        }
        private bool SelectTarget(Animator animator, ref Vector3 target)
        {
            var patrolCenter = patrolStats.patrolCenter;
            var patrolArea = patrolStats.patrolArea;

            var randX = Random.Range(patrolCenter.x - patrolArea.x, patrolCenter.x + patrolArea.x);
            var randY = Random.Range(patrolCenter.z - patrolArea.y, patrolCenter.z + patrolArea.y);
            var castPos = new Vector3(randX, 100, randY);

            if (Physics.Raycast(castPos, Vector3.down, out var hit, 110, mask))
            {
                if (hit.collider.tag == "Water") return false;
                target = hit.point;
                return true;
            }
            return false;
        }
    }
}
