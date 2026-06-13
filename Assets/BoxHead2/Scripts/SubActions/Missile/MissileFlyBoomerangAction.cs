using System;
using BoxHead2.Actor;
using BoxHead2.Helper;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using UnityEngine;
using Event = Dacodelaac.Events.Event;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileFlyBoomerangAction")]
    public class MissileFlyBoomerangAction : MissileFlyAction
    {
        [SerializeField] Event boomerangComebackHitEvent;
        [SerializeField] float speed;
        [SerializeField] float acceleration = 20;
        [SerializeField] float backSpeed;
        [SerializeField] float backAcceleration = 20;
        [SerializeField] bool penetrate = true;
        [Header("Feedback")]
        [SerializeField] Feedback hitObstacleFeedback;
        [SerializeField] protected ParticleSystem hitFxPrefab;
        
        float ForwardSpeed => speed * (1 + rangeBonusValue);
        float BackwardSpeed => backSpeed * (1 + rangeBonusValue);
        float Acceleration => acceleration;
        float BackAcceleration => backAcceleration;
        
        protected BoomerangState State;
        protected Missile Missile;
        protected float FlySpeed;
        
        IActor actor;
        Vector3 direction;
        Vector3 destination;
        Vector3 reachDestinationPos;
        float range;
        float rangeBonusValue;

        public override void Trigger(object target, IActor actor)
        {
            switch (State)
            {
                case BoomerangState.FlyForward:
                    OnFlyForward();
                    break;
                case BoomerangState.FlyBackward:
                    OnFlyBackward();
                    break;
                case BoomerangState.Stop:
                    OnStop();
                    break;
                default:
                    break;
            }
        }

        void OnFlyForward()
        {
            Missile.Transform.position = Vector3.MoveTowards(Missile.Transform.position, destination, FlySpeed * Time.deltaTime);
            Missile.CurrentDirection = direction;
            FlySpeed -= Acceleration * Time.deltaTime;
            if (FlySpeed <= 0 || Missile.Transform.position == destination)
            {
                FlySpeed = 0;
                OnReachDestination();
            }
        }

        void OnFlyBackward()
        {
            if (actor != null && !actor.Destroyed && actor.Alive)
            {
                var targetPos = actor.LockPosition;
                //targetPos.y = destination.y;
                
                var position = Missile.Transform.position;
                direction = targetPos - position;
                    
                position = Vector3.MoveTowards(position, targetPos, FlySpeed * Time.deltaTime);
                    
                Missile.Transform.position = position;
                Missile.CurrentDirection = direction;
                    
                FlySpeed = Mathf.Min(BackwardSpeed, FlySpeed + BackAcceleration * Time.deltaTime);
                    
                if (SimpleMath.InRange(Missile.Transform.position, targetPos, 0.1f))
                {
                    Missile.Despawn();
                }
            }
            else
            {
                Missile.Despawn();
            }
        }

        protected virtual void OnStop()
        {
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            actor = data.SetupData.Actor;
            direction = data.SetupData.Direction;
            var source = data.SetupData.Source;
            range = data.SetupData.Range;
            destination = source + direction * range;
            Missile = data.Missile;
            Missile.Transform.position = source;
            Missile.Transform.rotation = Quaternion.LookRotation(direction);
            State = BoomerangState.FlyForward;
            FlySpeed = ForwardSpeed;
            rangeBonusValue = 0;
        }

        public override void OnHit(object target)
        {
            switch (State)
            {
                case BoomerangState.FlyForward:
                    if (!penetrate)
                    {
                        OnReachDestination();
                    }
                    break;
                case BoomerangState.FlyBackward:
                    if (boomerangComebackHitEvent)
                    {
                        var data = Get<MissileHitActionData>(target);
                        if (data.DamageTaker is Actor.Actor)
                        {
                            boomerangComebackHitEvent.Raise();
                        }
                    }
                    break;
                case BoomerangState.Stop:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnHitObstacle(object target)
        {
            var data = Get<MissileHitActionData>(target);
            FxHelper.SpawnFx(pools, data.HitPosition, data.HitDirection, hitFxPrefab, hitObstacleFeedback);
            switch (State)
            {
                case BoomerangState.FlyForward:
                    OnReachDestination();
                    break;
                case BoomerangState.FlyBackward:
                    if (!SimpleMath.InRange(reachDestinationPos, Missile.Transform.position, 1f))
                    {
                        Missile.Despawn();
                    }
                    break;
                case BoomerangState.Stop:
                    break;
                default:
                    break;
            }
        }

        public override void OnMissileDeSpawn(object target)
        {
        }

        protected virtual void OnReachDestination()
        {
            if (State != BoomerangState.FlyForward) return;
            DoComeback();
        }

        protected void DoComeback()
        {
            State = BoomerangState.FlyBackward;
            reachDestinationPos= Missile.Transform.position;
            Missile.ResetHit();
        }
    }
    
    public enum BoomerangState
    {
        FlyForward,
        FlyBackward,
        Stop
    }
}