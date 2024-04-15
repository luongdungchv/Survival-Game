using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Enemy.Strategies
{
    public class RaycastGroundNormal : IStrategy
    {
        private Vector3 castPos;
        private float castDist;
        private LayerMask mask;
        private Vector3 _groundNormal;
        public Vector3 groundNormal
        {
            get
            {
                this.Excute();
                return _groundNormal;
            }
        }
        public RaycastGroundNormal(Vector3 castPos, float castDist, LayerMask mask)
        {
            this.castDist = castDist;
            this.castPos = castPos;
            this.mask = mask;
        }
        public void Excute()
        {
            if (Physics.Raycast(castPos, Vector3.down, out var hit, castDist, mask))
            {
                _groundNormal = hit.normal;
            }
            else _groundNormal = Vector3.up;
        }
    }
    public class ChaseStrategy
    {
        private float chaseRange;
        private float atkRange;
        private Transform target, chaser;
        private Animator chaserAnim;

        public ChaseStrategy(float chaseRange, float atkRange, Transform target, Transform chaser)
        {
            this.chaseRange = chaseRange;
            this.atkRange = atkRange;
            this.target = target;
            this.chaser = chaser;
            this.chaserAnim = chaser.GetComponent<Animator>();
        }
        public void Excute()
        {
            var distToTarget = Vector3.Distance(target.position, chaserAnim.transform.position);
            if (distToTarget < chaseRange && distToTarget > atkRange + 1)
            {
                chaserAnim.SetBool("chase", true);
            }
            else if (distToTarget < atkRange - 1)
            {
                chaserAnim.SetBool("chase", false);
                chaserAnim.SetBool("atk", true);
            }
            else
            {
                chaserAnim.SetBool("chase", false);
            }
        }
    }
    public interface IStrategy
    {
        void Excute();
    }

}

