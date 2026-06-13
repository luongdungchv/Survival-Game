using BoxHead2.States;
using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.Actor
{
    public class TwinRatsBaseRunAwayState : AIBaseRunAwayState
    {
        public TwinRatsBaseRunAwayState(AIEnemy aiEnemy) : base(aiEnemy)
        {
            
        }

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            Actor.EnterStealth();
            Actor.ToggleRunTraceFX(true);
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            Actor.ExitStealth();
            Actor.ToggleRunTraceFX(false);
        }
    }
}