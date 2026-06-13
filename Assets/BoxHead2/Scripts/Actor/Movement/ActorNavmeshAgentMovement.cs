using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Actor
{
    public class ActorNavmeshAgentMovement : BaseMono, IActorMovement
    {
        [SerializeField] bool isNoneQuality;
        public float Radius => Agent.radius;
        
        NavMeshAgent _agent;
        NavMeshAgent Agent => gameObject.GetAndCacheComponent(ref _agent);

        bool _isFixedAvoidancePriority;

        public void InitProperties(int avoidancePriorityNormal, float acceleration)
        {
            Agent.acceleration = acceleration;
            Agent.avoidancePriority = avoidancePriorityNormal;
            _isFixedAvoidancePriority = false;
            EnableMovementCollision();
        }

        public void MovePosition(Vector3 position, float speed, float rotateSpeed, float stoppingDistance)
        {
            if (!Agent.isActiveAndEnabled) return;
            
            Agent.stoppingDistance = stoppingDistance;
            Agent.speed = speed;
            if (rotateSpeed < 0)
            {
                Agent.updateRotation = false;
            }
            else
            {
                Agent.updateRotation = true;
                Agent.angularSpeed = rotateSpeed * 360f;   
            }
            Agent.destination = position;
        }

        public void MoveDirection(Vector3 direction, float speed)
        {
            direction.y = 0;
            direction.Normalize();
            Agent.speed = speed;
            Agent.velocity = speed * direction;
        }

        public void Rotate(Vector3 direction, float rotateSpeed, bool immediately = false)
        {
            Agent.updateRotation = false;
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
            if (Agent.isActiveAndEnabled)
            {
                Agent.ResetPath();
                Agent.velocity = Vector3.zero;
            }
        }
        
        public void Warp(Vector3 pos)
        {
            Agent.enabled = true;
            Agent.Warp(pos);
        }

        public void OnSkillBeginMove(int avoidancePriorityAttack)
        {
            if (_isFixedAvoidancePriority) return;
            if (Agent)
            {
                Agent.avoidancePriority = avoidancePriorityAttack;
            }
        }

        public void OnSkillStopMove(int avoidancePriorityNormal)
        {
            if (_isFixedAvoidancePriority) return;
            if (Agent)
            {
                Agent.avoidancePriority = avoidancePriorityNormal;
            }
        }

        public void FixedAvoidancePriority(bool isActive, int priority)
        {
            _isFixedAvoidancePriority = isActive;
            if (Agent)
            {
                Agent.avoidancePriority = priority;
            }
        }

        public void Move(Vector3 amount)
        {
            if (Agent.isActiveAndEnabled)
            {
                Agent.Move(amount);
            }
        }
        
        public void OnHiding()
        {
            Agent.enabled = false;
        }

        public void OnUnHide()
        {
            Agent.enabled = true;
        }

        public bool IsReachDestination()
        {
            if (Agent)
            {
                return Agent.IsReachDestination();
            }

            return false;
        }

        public void DisableMovement()
        {
            Agent.enabled = false;
        }

        public void EnableMovement()
        {
            Agent.enabled = true;
        }

        public void OnBeginRoll()
        {
            Agent.enabled = false;
        }

        public void OnStopRoll()
        {
            Agent.enabled = true;
        }
        
        public void DisableMovementCollision()
        {
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        
        public void EnableMovementCollision()
        {
            Agent.obstacleAvoidanceType = isNoneQuality ? ObstacleAvoidanceType.NoObstacleAvoidance : ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        }
        
        public void ChangeRadius(float r)
        {
            Agent.radius = r;
        }
    }
}