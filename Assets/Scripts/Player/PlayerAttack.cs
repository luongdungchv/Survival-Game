using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack ins;
    private Tool _currentWield;
    public string currentWieldName => _currentWield.itemName;
    public float currentBaseDmg => _currentWield.baseDmg;
    public Tool currentWield { set => _currentWield = value; }
    [SerializeField] private List<ParticleSystem> slashFXList;
    //[SerializeField] private DamageDealer attacker;

    PlayerMovement movementSystem;
    PlayerAnimation animManager;

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
    private void Awake()
    {
        if (ins == null) ins = this;
    }


    public void StopAnimationCountdown()
    {
        if (animCountdown != null) StopCoroutine(animCountdown);
    }
    public void ResetAttack()
    {
        attackIndex = -1;
        animManager.CancelAttack(pattern.type);
    }
    public void PerformAttack()
    {
        var inputReader = InputReader.ins;
        var init = GetComponent<StateInitializer>();
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

            animManager.PerformAttack(fxIndex, pattern.type);

            var displaced = pattern.displaceForward[attackIndex];
            movementSystem.DisplaceForward(pattern.displaceForward[attackIndex]);

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

        if (inputReader.SlashPress())
        {
            if (mouseWaitCountdown != null) StopCoroutine(mouseWaitCountdown);
            mouseWaitCountdown = StartCoroutine(WaitForClick(0.43f));

            if (animCountdown != null) StopCoroutine(animCountdown);
            animCountdown = StartCoroutine(AnimationCountdown());

            if (!isInAttackingPhase)
                StartCoroutine(attackCoroutine());
        }
    }

    //Used as animation event
    public void DetectHit(int hitIndex)
    {
        pattern.DetectHit(hitIndex);
    }
    public void SetAtkPattern(AttackPattern pattern)
    {
        this.pattern = pattern;
    }
}
