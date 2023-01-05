using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAttack : MonoBehaviour
{
    private InputReceiver inputReceiver;
    private AttackPattern pattern;
    private int attackIndex = -1;
    private bool isInAttackingPhase;
    private Coroutine mouseWaitCountdown, animCountdown;

    private PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    private void Awake()
    {
        inputReceiver = GetComponent<InputReceiver>();
    }
    public void ResetAttack()
    {
        attackIndex = -1;
        animSystem.CancelAttack(pattern.type);
    }
    public void AttackServer()
    {

        var init = GetComponent<NetworkStateManager>();
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
            Debug.Log(fxIndex);

            animSystem.PerformAttack(fxIndex, pattern.type);

            var displaced = pattern.displaceForward[attackIndex];
            //movementSystem.DisplaceForward(pattern.displaceForward[attackIndex]);

            var delay = pattern.delayBetweenMoves[attackIndex];
            yield return new WaitForSeconds(delay);

            isInAttackingPhase = false;

        }
        IEnumerator WaitForClick(float duration)
        {
            init.Attack.lockState = true;
            yield return new WaitForSeconds(duration);
            init.Attack.lockState = false;
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
