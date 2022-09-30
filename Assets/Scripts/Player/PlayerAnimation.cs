using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    Coroutine animationBlendCoroutine;
    private Dictionary<string, Coroutine> animBlendCoroutineDict;
    public Animator animator;

    PlayerMovement movementSystem;
    void Start()
    {
        //animator = GetComponent<Animator>();
        movementSystem = GetComponent<PlayerMovement>();
        animBlendCoroutineDict = new Dictionary<string, Coroutine>();
        foreach (var i in animator.parameters)
        {
            var name = i.name;
            animBlendCoroutineDict.Add(name, null);
        }
    }


    public void PerformJog()
    {

    }
    public void PerformRun()
    {

    }
    public void PerformAttack(int index)
    {
        animator.SetBool("slash", true);
        animator.SetInteger("attack", index);
    }
    public void CancelAttack()
    {
        animator.SetBool("slash", false);
    }
    public void Walk()
    {
        BlendAnimation("move", 0.5f, 0.2f);
    }
    public void Run()
    {
        BlendAnimation("move", 1, 0.4f);
        //animator.SetFloat("move", 1);
    }
    public void Jump()
    {
        animator.SetBool("jump", true);
    }
    public void CancelJump()
    {
        //Debug.Log("cancelJump");
        animator.SetBool("jump", false);
    }
    public void Idle()
    {
        BlendAnimation("move", 0, 0.2f);
        BlendAnimation("swim", 0, 0.2f);

        //animator.SetBool("jump", false);
    }
    public void SwimIdle()
    {
        BlendAnimation("move", 0, 0);
        BlendAnimation("swim", 0.15f, 0.2f);
    }
    public void SwimNormal()
    {
        BlendAnimation("move", 0, 0);
        BlendAnimation("swim", 0.5f, 0.2f);
    }
    public void SwimFast()
    {

    }

    IEnumerator LerpAnimationTransition(string param, float to, float duration)
    {
        float t = 0;
        float from = animator.GetFloat(param);
        while (t <= 1)
        {
            t += Time.deltaTime / duration;
            float state = Mathf.Lerp(from, to, t);
            animator.SetFloat(param, state);
            yield return null;
        }
        if (t > 1) animator.SetFloat(param, to);
    }
    void BlendAnimation(string param, float to, float duration)
    {
        if (animator.GetFloat(param) == to) return;
        var animCoroutine = animBlendCoroutineDict[param];
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }
        //StopCoroutine(animationBlendCoroutine);
        animBlendCoroutineDict[param] = StartCoroutine(LerpAnimationTransition(param, to, duration));
    }
}
