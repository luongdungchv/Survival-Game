using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class DeadBaseAnimAndExplodeState : DeadBaseState
    {
        float _t;
        public DeadBaseAnimAndExplodeState(Actor actor) : base(actor)
        {
        }
        
        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);

            Actor.StopMovement();
            Actor.PlayAnimation("die", 0, 1, false, false);
            _t = Time.time;
            Actor.OnDead();
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (Time.time - _t > Actor.Config.deadDuration)
            {
                Actor.DeadExplode();
            }
        }
    }
}