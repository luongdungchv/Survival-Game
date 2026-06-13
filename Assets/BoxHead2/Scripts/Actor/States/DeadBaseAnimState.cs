using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.Actor
{
    public class DeadBaseAnimState : DeadBaseState
    {
        public DeadBaseAnimState(Actor actor) : base(actor)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);

            Actor.StopMovement();
            Actor.PlayAnimation("die", 0, 1, false, false);
            Actor.OnDead();
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            Actor.OnRevive();
        }
    }
}