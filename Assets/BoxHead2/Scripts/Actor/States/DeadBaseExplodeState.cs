using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.States
{
    public class DeadBaseExplodeState : DeadBaseState
    {
        public DeadBaseExplodeState(Actor.Actor Actor) : base(Actor)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            Actor.DeadExplode();
        }
    }
}