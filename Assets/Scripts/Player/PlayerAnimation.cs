using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

public class PlayerAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Image bloodOverlay;
    Coroutine animationBlendCoroutine;
    private Dictionary<string, Coroutine> animBlendCoroutineDict;
    public Animator animator;

    PlayerMovement movementSystem;
    PlayerAttack atkSystem;
    PlayerStats stats;
    
    private float dashTime => stats.dashTime;
    void Start()
    {
        //animator = GetComponent<Animator>();
        movementSystem = GetComponent<PlayerMovement>();
        atkSystem = GetComponent<PlayerAttack>();
        animBlendCoroutineDict = new Dictionary<string, Coroutine>();
        stats = GetComponent<PlayerStats>();
        foreach (var i in animator.parameters)
        {
            var name = i.name;
            animBlendCoroutineDict.Add(name, null);
        }
    }
    public void PerformAttack(int index, string type)
    {
        animator.SetBool(type, true);
        animator.SetInteger("attack", index);
    }
    public void CancelAttack(string attackType)
    {
        animator.SetFloat("attack", -1);
        animator.SetBool(attackType, false);
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

    }
    public void SwimIdle()
    {
        BlendAnimation("move", 0, 0);
        animator.SetFloat("swim", 0.5f);
        BlendAnimation("swim", 0.15f, 0.15f);
    }
    public void SwimNormal()
    {
        BlendAnimation("move", 0, 0);
        BlendAnimation("swim", 0.5f, 0.15f);
    }
    public async void Dash()
    {
        animator.SetBool("dash", true);

        await Task.Delay((int)(this.dashTime * 1000));
        animator.SetBool("dash", false);
    }
    public void HurtEffect(float appearDuration, float fadeDuration){
        IEnumerator Animate(float duration, int state){
            float t = 0;
            while(t < 1){
                t += Time.deltaTime / duration;
                var bloodColor = bloodOverlay.color;
                bloodColor.a = Mathf.Abs(state - t);
                bloodOverlay.color = bloodColor;
                yield return null;
            }
            if(state == 0) StartCoroutine(Animate(fadeDuration, 1));
            else{
                var bloodColor = bloodOverlay.color;
                bloodColor.a = 0;
                bloodOverlay.color = bloodColor;
            }   
        }
        StartCoroutine(Animate(appearDuration, 0));
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
        animBlendCoroutineDict[param] = StartCoroutine(LerpAnimationTransition(param, to, duration));
    }
}
