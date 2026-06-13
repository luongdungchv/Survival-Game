using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Actor
{
    public class ActorObstacleMovement : BaseMono, IActorMovement
    {
        [SerializeField] float radius = 1;
        public float Radius => radius;
        
        [SerializeField] Transform rotatePivot;
        [SerializeField] float rotateOffset;

        NavMeshObstacle _navMeshObstacle;
        NavMeshObstacle NavMeshObstacle => gameObject.GetAndCacheComponent(ref _navMeshObstacle); 
        
        public void InitProperties(int avoidancePriorityNormal, float acceleration)
        {
        }

        public void MovePosition(Vector3 position, float speed, float rotateSpeed, float stoppingDistance)
        {
        }

        public void MoveDirection(Vector3 direction, float speed)
        {
        }

        public void Rotate(Vector3 direction, float rotateSpeed, bool immediately = false)
        {
            if (rotatePivot)
            {
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    if (immediately)
                    {
                        rotatePivot.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotateOffset, 0);
                    }
                    else
                    {
                        rotatePivot.rotation = Quaternion.Slerp(rotatePivot.rotation,
                            Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotateOffset, 0),
                            rotateSpeed * Time.deltaTime);
                    }
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
            NavMeshObstacle.enabled = false;
        }

        public void EnableMovement()
        {
            NavMeshObstacle.enabled = true;
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

        public void SetRotatePivot(Transform t)
        {
            rotatePivot = t;
        }
    }
}