using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> slashFXList;
    [SerializeField] private DamageDealer attacker;

    PlayerMovement movementSystem;
    PlayerAnimation animManager;

    [SerializeField] private int attackMoves;
    [SerializeField] private float delayBetweenMoves, resetDelay;
    [SerializeField] private AttackPattern pattern;
    int attackIndex = -1;
    bool isInAttackingPhase;

    Coroutine mouseWaitCountdown;
    Coroutine animCountdown;
    // Start is called before the first frame update
    void Start()
    {
        animManager = GetComponent<PlayerAnimation>();
        movementSystem = GetComponent<PlayerMovement>();
        attackIndex = -1;
        isInAttackingPhase = false;
    }

    // Update is called once per frame
    void Update()
    {
        //PerformAttack();
    }


    public void StopAnimationCountdown()
    {
        if (animCountdown != null) StopCoroutine(animCountdown);
    }
    public void ResetAttack()
    {
        attackIndex = -1;
        animManager.CancelAttack(pattern.type);
        attacker.transform.parent.gameObject.SetActive(false);
    }
    public void PerformAttack(InputReader inputReader, StateInitializer init)
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

            animManager.PerformAttack(fxIndex);

            var displaced = pattern.displaceForward[attackIndex];
            movementSystem.DisplaceForward(pattern.displaceForward[attackIndex]);

            attacker.transform.parent.gameObject.SetActive(true);
            attacker.EnableWeapon();

            var delay = pattern.delayBetweenMoves[attackIndex];
            yield return new WaitForSeconds(delay);

            attacker.DisableWeapon();

            isInAttackingPhase = false;

            //slashFXList[fxIndex].Play();           


        }
        IEnumerator WaitForClick(float duration)
        {
            init.Attack.lockState = true;
            yield return new WaitForSeconds(duration);
            init.Attack.lockState = false;
        }

        if (inputReader.SlashPress())
        {
            //StartCoroutine(EnableDetection());
            if (mouseWaitCountdown != null) StopCoroutine(mouseWaitCountdown);
            mouseWaitCountdown = StartCoroutine(WaitForClick(0.43f));

            if (animCountdown != null) StopCoroutine(animCountdown);
            animCountdown = StartCoroutine(AnimationCountdown());

            if (!isInAttackingPhase)
                StartCoroutine(attackCoroutine());
        }
    }
}
