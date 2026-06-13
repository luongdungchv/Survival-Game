using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.Actor
{
    public class StaggerBaseState : ActorBaseState
    {
        bool _isRaising;
        public StaggerBaseState(Actor actor) : base(actor)
        {
        }
        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            Actor.StopMovement();
            Actor.PlayAnimation("break", 0, 1, false, false);
            _isRaising = false;

        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (!_isRaising && !Actor.IsStaggering)
            {
                _isRaising = true;
                if (Actor.Config.riseAfterBreak)
                {
                    Actor.PlayAnimation("rise", 0, 1, true, false);
                }
                else
                {
                    Actor.PlayAnimation("locomotion", 0, 1, true, true);
                }
            }
            if (_isRaising)
            {
                if (!Actor.IsPlayingAnim)
                {
                    Actor.OnBreakEnded();
                    ChangeState<IIdleState>();
                }
            }
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            _isRaising = false;

        }
    }
}