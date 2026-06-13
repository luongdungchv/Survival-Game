using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.States
{
    public class EmptyBaseState : ActorBaseState
    {
        public EmptyBaseState(Actor.Actor actor) : base(actor)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            Actor.StopMovement();
        }
    }
}