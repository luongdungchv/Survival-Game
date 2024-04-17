﻿using System.Collections;
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
            var netObj = animator.GetComponent<NetworkSceneObject>();
            if (Client.ins.isHost && target != null && target.TryGetComponent<NetworkPlayer>(out var targetNetObj))
            {
                var updatePacket = new RawActionPacket(PacketType.UpdateEnemy)
                {
                    playerId = "0",
                    objId = netObj.id,
                    action = "set_target",
                    actionParams = new string[] { targetNetObj.id }
                };
                Client.ins.SendTCPPacket(updatePacket);
            }
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(target == null || target.GetComponent<PlayerStats>().isDead){
                animator.ResetTrigger("chase");
                animator.SetTrigger("patrol");
            }
            if(navAgent.navMeshOwner == null) return;
            try{
                navAgent.destination = target.position;
            }
            catch{
            }
        }
    }
}