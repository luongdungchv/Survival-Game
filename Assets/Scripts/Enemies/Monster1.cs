using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster1 : MonoBehaviour
{
    public enum State { Idle, Attack};

    public float speed;
    public float hp;
    public State currentState;

    Rigidbody rb;
    Animator animator;

    Coroutine moveAroundCoroutine;
    Coroutine moveCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        //rb.MovePosition(transform.forward * 2);
        //moveAroundCoroutine = StartCoroutine(MoveEnum(transform.position + transform.forward * 12));
        //rb.velocity = transform.forward * 2;
        Debug.Log("sdf");
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
   
    public void MoveAround()
    {
        moveAroundCoroutine = StartCoroutine(MoveAroundEnum());
    }
    public void AttackPlayer()
    {
        if (moveAroundCoroutine != null) StopCoroutine(moveAroundCoroutine);
    }
    void PerformAttack()
    {

    }
    IEnumerator MoveAroundEnum()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        animator.SetBool("idle", false);
        moveCoroutine = StartCoroutine(MoveEnum(transform.position + transform.forward * 10));
        yield return new WaitForSeconds(3);
        //rb.velocity = Vector3.zero;
        animator.SetBool("idle", true);
        yield return new WaitForSeconds(2);
        animator.SetBool("idle", false);
        transform.RotateAround(transform.position, Vector3.up, 60);
        moveAroundCoroutine = StartCoroutine(MoveAroundEnum());
    }
    IEnumerator MoveEnum(Vector3 dest)
    {
        float t = 0;
        var initialPos = transform.position;
        while (t <= 1)
        {
            t += Time.deltaTime / 3;
            transform.position = Vector3.Lerp(initialPos, dest, t);
            yield return null;
        }
    }
    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("hurt");
    }
    private void OnParticleTrigger()
    {
        Debug.Log("sdsfg");
    }
}
