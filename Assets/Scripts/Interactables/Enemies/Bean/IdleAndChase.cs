using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using UnityEngine.AI;
using UnityEngine.Animations;

public class IdleAndChase : StateMachineBehaviour
{
    private Transform target;
    private IPatrol patrolStats;
    private IChase chaseStats;
    private NavMeshAgent navAgent;
    private Dictionary<string, NetworkPlayer> players;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var stats = animator.GetComponent<EnemyStats>();
        navAgent = animator.GetComponent<NavMeshAgent>();
        target = stats.target;
        patrolStats = stats as IPatrol;
        chaseStats = stats as IChase;
        navAgent.isStopped = false;
        players = NetworkManager.ins.GetAllPlayers();
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        if(target == null){
            foreach(var i in players.Keys){
                var distance = Vector3.Distance(animator.transform.position, players[i].transform.position);
                if(distance < chaseStats.chaseRange){
                    target = players[i].transform;
                    return;
                }         
            }
        }
        else{
            var distance = Vector3.Distance(animator.transform.position,target.position);
            if(distance > chaseStats.chaseRange) {
                target = null;
                return;
            }
        }
    }
}
