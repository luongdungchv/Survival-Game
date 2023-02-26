using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Bean
{
    public class IdleBehaviour : StateMachineBehaviour
    {
        private float elapsed;
        private NavMeshAgent navAgent;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            elapsed = 0;
            navAgent = animator.GetComponent<NavMeshAgent>();
            navAgent.isStopped = true;
            var netObj = animator.GetComponent<NetworkSceneObject>();
            if (Client.ins.isHost)
            {
                var updatePacket = new RawActionPacket(PacketType.UpdateEnemy)
                {
                    playerId = "0",
                    objId = netObj.id,
                    action = "idle",
                };
                Client.ins.SendTCPPacket(updatePacket);
            }

        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!Client.ins.isHost) return;
            elapsed += Time.deltaTime;
            if (elapsed >= 3)
            {
                animator.ResetTrigger("idle");
                animator.SetTrigger("patrol");
            }
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }
    }
}
