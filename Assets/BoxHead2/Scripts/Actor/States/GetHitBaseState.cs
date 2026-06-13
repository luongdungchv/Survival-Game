using BoxHead2.Combat;
using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class GetHitBaseState : ActorBaseState
    {
        float duration;

        public GetHitBaseState(Actor actor) : base(actor)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            var damageForce = (CombinedDamageForceData) data;
            duration = damageForce.Duration;
            var dir = Actor.Transform.InverseTransformDirection(damageForce.HitDirection);
            dir.y = dir.z;
            Actor.StopMovement();
            Actor.PlayAnimation("hit_react", Actor.IsPlayer ? 1: 0, 1, false, false);
            Actor.UpdateLocomotion(Vector2.zero);
            Actor.UpdateHitAnimDirection(dir);
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            duration -= Time.deltaTime;
            if (duration <= 0)
            {
                Actor.OnGetHitEnded();
                ChangeState<IIdleState>();
            }
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            Actor.SetAnimatorSpeed();
        }
    }
}