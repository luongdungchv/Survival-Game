using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class ActorBasicMovement : BaseMono, IActorMovement
    {
        [SerializeField] float radius = 1;
        public float Radius => radius;

        public void InitProperties(int avoidancePriorityNormal, float acceleration)
        {
        }

        public void MovePosition(Vector3 position, float speed, float rotateSpeed, float stoppingDistance)
        {
            var pos = Transform.position;
            Rotate(position - pos, rotateSpeed);
            pos = Vector3.MoveTowards(pos, position, speed * Time.deltaTime);
            Transform.position = pos;
        }

        public void MoveDirection(Vector3 direction, float speed)
        {
            direction.y = 0;
            direction.Normalize();
            Transform.position += speed * Time.deltaTime * direction;
        }

        public void Rotate(Vector3 direction, float rotateSpeed, bool immediately = false)
        {
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                if (immediately)
                {
                    Transform.rotation = Quaternion.LookRotation(direction);
                }
                else
                {
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(direction),
                        rotateSpeed * Time.deltaTime);
                }
            }
        }

        public void Stop()
        {
        }
        
        public void Warp(Vector3 pos)
        {
            Transform.position = pos;
        }

        public void OnSkillBeginMove(int avoidancePriorityAttack)
        {
        }

        public void OnSkillStopMove(int avoidancePriorityNormal)
        {
        }

        public void FixedAvoidancePriority(bool isActive, int priority)
        {
            
        }

        public void Move(Vector3 amount)
        {
            Transform.position += amount;
        }
        
        public void OnHiding()
        {
        }

        public void OnUnHide()
        {
        }

        public bool IsReachDestination()
        {
            return false;
        }

        public void DisableMovement()
        {
        }

        public void EnableMovement()
        {
        }

        public void OnBeginRoll()
        {
        }

        public void OnStopRoll()
        {
        }

        public void DisableMovementCollision()
        {
        }
        
        public void EnableMovementCollision()
        {
        }

        public void ChangeRadius(float r)
        {
            radius = r;
        }
    }
}