using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class BossDeadBaseAnimState : DeadBaseState
    {
        float deadDuration;
        bool dissolve;
        bool despawn;
        
        public BossDeadBaseAnimState(Actor actor) : base(actor)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);

            Actor.StopMovement();
            Actor.TurnOffHitBox();

            deadDuration = Actor.Config.deadDuration;
            dissolve = false;
            despawn = false;
            
            Actor.PlayAnimation("die", 0, 1, false, false);
            Actor.PlayDeadFeedback();
        }
        
        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            deadDuration -= Time.deltaTime;
            if (!dissolve && deadDuration < 0)
            {
                dissolve = true;
                deadDuration = 3;
                Actor.Dissolve(deadDuration);
            }
            if (dissolve)
            {
                if (!despawn && deadDuration < 0)
                {
                    despawn = true;
                    Actor.Despawn(Actor.Position);
                }   
            }
        }
    }
}