using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class NetworkAttack : MonoBehaviour
{
    private InputReceiver inputReceiver;
    [SerializeField] private AttackPattern pattern;
    public int attackIndex = -1;
    private bool isInAttackingPhase;
    private Coroutine mouseWaitCountdown, animCountdown;
    private NetworkStateManager stateManager;

    private PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    private void Awake()
    {
        stateManager = GetComponent<NetworkStateManager>();
        inputReceiver = GetComponent<InputReceiver>();
    }
    public void ResetAttack()
    {
        attackIndex = -1;
        stateManager.Attack.SetLock("Idle", false);
        animSystem.CancelAttack(pattern.type);
    }
    public void AttackServer()
    {
        IEnumerator AnimationCountdown()
        {
            var delay = pattern.resetDelay[attackIndex == pattern.attackCount - 1 ? 0 : attackIndex + 1];
            yield return new WaitForSeconds(delay);
            ResetAttack();
        }
        IEnumerator attackCoroutine()
        {
            isInAttackingPhase = true;

            if (attackIndex == pattern.attackCount - 1) attackIndex = -1;
            attackIndex++;

            int fxIndex = attackIndex;

            animSystem.PerformAttack(fxIndex, pattern.type);

            var displaced = pattern.displaceForward[attackIndex];

            var delay = pattern.delayBetweenMoves[attackIndex];
            yield return new WaitForSeconds(delay);

            isInAttackingPhase = false;

        }
        IEnumerator WaitForClick(float duration)
        {
            stateManager.Attack.LockAllTransitions();
            yield return new WaitForSeconds(duration);
            stateManager.Attack.UnlockAllTransitions();
            stateManager.Attack.SetLock("Idle", true);
        }
        if (inputReceiver.attack)
        {
            if (mouseWaitCountdown != null) StopCoroutine(mouseWaitCountdown);
            mouseWaitCountdown = StartCoroutine(WaitForClick(0.43f));

            if (animCountdown != null) StopCoroutine(animCountdown);
            animCountdown = StartCoroutine(AnimationCountdown());

            if (!isInAttackingPhase)
                StartCoroutine(attackCoroutine());
        }
    }
    public void SetAttackPattern(AttackPattern pattern)
    {
        this.pattern = pattern;
    }
}
