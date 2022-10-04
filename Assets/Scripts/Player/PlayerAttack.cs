using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public List<ParticleSystem> slashFXList;
    public List<AnimationClip> clipList;
    public DamageDealer attacker;
    public State atkState;

    public Animator animator;
    PlayerMovement movementSystem;
    PlayerAnimation animManager;

    public int attackMoves;
    public bool isAttacking;
    int attackIndex = -1;
    bool isInAttackingPhase;

    Coroutine mouseWaitCountdown;
    Coroutine animCountdown;
    // Start is called before the first frame update
    void Start()
    {
        animManager = GetComponent<PlayerAnimation>();
        movementSystem = GetComponent<PlayerMovement>();
        isAttacking = false;
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
        animator.SetFloat("attack", attackIndex);
        animator.SetBool("slash", false);
        attacker.transform.parent.gameObject.SetActive(false);
    }
    public void PerformAttack(InputReader inputReader, StateInitializer init)
    {

        IEnumerator AnimationCountdown()
        {
            yield return new WaitForSeconds(1);
            ResetAttack();
        }
        IEnumerator attackCoroutine(float duration)
        {
            isInAttackingPhase = true;

            if (attackIndex == attackMoves) attackIndex = -1;
            attackIndex++;

            int fxIndex = attackIndex;

            animManager.PerformAttack(fxIndex);

            //movementSystem.StopMoving();
            //movementSystem.isStartMove = true;

            attacker.transform.parent.gameObject.SetActive(true);
            attacker.EnableWeapon();

            yield return new WaitForSeconds(duration);

            attacker.DisableWeapon();

            isInAttackingPhase = false;

            //slashFXList[fxIndex].Play();           

            //BlendAnimation("attack", 1, 0.15f);

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

            if (!isInAttackingPhase) StartCoroutine(attackCoroutine(0.28f));
        }
    }
}
