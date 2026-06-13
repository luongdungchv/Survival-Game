using Dacodelaac.FiniteStateMachine;

namespace BoxHead2.Actor
{
    public class KnockdownBaseState : ActorBaseState
    {
        bool _isRaising;
        public KnockdownBaseState(Actor actor) : base(actor)
        {
        }
        

        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            Actor.PlayAnimation("knock_down", 0, 1, true, false);
            Actor.RotateDirection(-Actor.DamageForce.HitDirection, 0, true);
            _isRaising = false;
            if (Actor.IsPlayer)
            {
                Actor.TurnOffWeapon();
            }
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            Actor.RotateDirection(-Actor.DamageForce.HitDirection, Actor.RotateSpeed);
            
            if (!_isRaising && !Actor.IsKnockingDown)
            {
                _isRaising = true;
                Actor.PlayAnimation("rise", 0, 1, true, false);
                Actor.OnKnockDownRaising();
            }
            if (_isRaising)
            {
                if (!Actor.IsPlayingAnim)
                {
                    Actor.OnKnockDownEnded();
                    if (Actor.IsStaggering)
                    {
                        Actor.RestoreStaggerAfterKnockdown();
                    }
                    else
                    {
                        ChangeState<IIdleState>();
                    }
                }
            }
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            _isRaising = false;
            if (Actor.IsPlayer)
            {
                Actor.TurnOnWeapon();
            }
        }

    }
}