using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.Actor
{
    public class ActorBaseState : Dacodelaac.FiniteStateMachine.BaseState
    {
        protected Actor Actor;

        public ActorBaseState(Actor actor)
        {
            Actor = actor;
        }
    }
}