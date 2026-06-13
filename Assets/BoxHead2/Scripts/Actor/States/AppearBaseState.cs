using BoxHead2.Actor;
using Dacodelaac.FiniteStateMachine;
using DG.Tweening;
using UnityEngine;

namespace BoxHead2.States
{
    public class AppearBaseState : ActorBaseState
    {
        public AppearBaseState(Actor.Actor Actor, bool skip = false) : base(Actor)
        {
            _skip = skip;
        }

        bool _skip;
        float spawnTime;
        bool jumped;
        Tween t;

        protected override void OnStateEnter(Dacodelaac.FiniteStateMachine.BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            if (_skip)
            {
                ChangeState<IIdleState>();
                return;
            }
            Actor.DisableMovement();
            Actor.TurnOffHitBox();
            // Actor.IsForceInvincible = true;
            // Actor.IsInvisible = true;
            
            if (Actor.Config.hasSpawnAnimation)
            {
                Actor.PlayAnimation("appear", 0, 1, false, false);
            }
            else
            {
                Actor.transform.position += Vector3.down * (Actor.HeadPosition.y + 3);
            }

            spawnTime = Time.time;
            jumped = false;
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (Actor.Config.hasSpawnAnimation)
            {
                if (!Actor.IsPlayingAnim)
                {
                    ChangeState<IIdleState>();
                }
            }
            else
            {
                if (!jumped && Time.time - spawnTime > 1f)
                {
                    jumped = true;
                    var pos = Actor.transform.position;
                    pos.y = 0;
                    Actor.transform.DOJump(pos, 2, 1, 0.5f).SetTarget(Actor.transform).OnComplete(() =>
                    {
                        ChangeState<IIdleState>();    
                    }).Play();
                }
            }
        }

        protected override void OnStateExit(Dacodelaac.FiniteStateMachine.BaseState to)
        {
            base.OnStateExit(to);
            Actor.EnableMovement();
            Actor.TurnOnHitBox();
            // Actor.IsInvisible = false;
            // Actor.IsForceInvincible = false;
            DOTween.Kill(Actor.transform);
        }
    }
}