using UnityEngine;

namespace BoxHead2.Actor
{
    public interface IActorMovement
    {
        public void InitProperties(int avoidancePriorityNormal, float acceleration);

        public void MovePosition(Vector3 position, float speed, float rotateSpeed, float stoppingDistance);

        public void MoveDirection(Vector3 direction, float speed);

        public void Rotate(Vector3 direction, float rotateSpeed, bool immediately = false);

        public void Stop();

        public void Warp(Vector3 pos);

        public void OnSkillBeginMove(int avoidancePriorityAttack);

        public void OnSkillStopMove(int avoidancePriorityNormal);
        public void FixedAvoidancePriority(bool isActive, int priority);

        public void Move(Vector3 amount);

        public void OnHiding();

        public void OnUnHide();

        public bool IsReachDestination();

        public void DisableMovement();

        public void EnableMovement();
        public void OnBeginRoll();
        public void OnStopRoll();
        public void DisableMovementCollision();
        public void EnableMovementCollision();
        public float Radius { get; }
        public void ChangeRadius(float radius);
    }
}