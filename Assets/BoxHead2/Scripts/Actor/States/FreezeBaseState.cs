using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.States
{
    public class FreezeBaseState : ActorBaseState
    {
        float thawDamage;
        bool thaw;
        
        public FreezeBaseState(Actor.Actor Actor) : base(Actor)
        {
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            // thawDamage = Actor.ThawDamage;
            thaw = false;
        }

        // protected override void OnStateUpdate()
        // {
        //     base.OnStateUpdate();
        //     if (!Actor.IsFreezing)
        //     {
        //         Thaw();
        //     }
        // }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            Thaw();
        }

        public void Thaw()
        {
            if (!thaw)
            {
                thaw = true;
                // Actor.OnThaw(thawDamage);
            }
            if (StateMachine.CurrentState == this)
            {
                ChangeState<IIdleState>();
            }
        }
    }
}