using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;

namespace Enemy.Base
{
    public class EnemyStats : MonoBehaviour
    {
        public float atkRange, closeRange;
        public float hp;
        public Transform target;
        private Rigidbody rb;
        public float distanceToTarget => Vector3.Distance(transform.position, target.position);
        private Animator animator;
        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
        }
        private void Update()
        {
            CustomUpdate();
        }
        protected virtual void CustomUpdate()
        {

        }
    }
    public interface IPatrol : IMove
    {
        Vector2 patrolArea { get; }
        Vector3 patrolCenter { get; set; }
    }
    public interface IMove
    {
        float runSpeed { get; set; }
        float walkSpeed { get; set; }
    }
    public interface IChase : IMove
    {
        float chaseRange { get; set; }
    }
    public interface IGround
    {
        Vector3 groundNormal { get; }
    }

}