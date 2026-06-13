using BoxHead2.Actor;
using BoxHead2.Helper;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public class MissileFlyBounceAction : MissileFlyAction
    {
        [Header("Bounce Missile Properties")]
        [SerializeField] float speed;
        [SerializeField] float extendRange;
        [SerializeField] int bounceCount;
        [SerializeField] Vector3 startingOffset;
        [SerializeField] protected float limitHeight = 999;
        [SerializeField] protected Feedback hitObstacleFeedback;
        [SerializeField] protected ParticleSystem hitFxPrefab;
        protected Missile Missile;
        protected Vector3 Destination;

        int _currentBounceRemain;
        Vector3 _direction;
        
        protected virtual float Speed => speed;
        public override void Trigger(object target, IActor actor)
        {
            var pos = Missile.Transform.position;
            pos = Vector3.MoveTowards(pos, Destination, Speed * Time.deltaTime);
            if (pos.y > limitHeight)
            {
                pos.y = Mathf.Lerp(pos.y, limitHeight, 10 * Time.deltaTime);
            }
            Missile.Transform.position = pos;
            Missile.CurrentDirection = Missile.Transform.forward;
            if (SimpleMath.InRange(Missile.Transform.position, Destination, 0.1f))
            {
                Missile.Despawn();
            }
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            SetupMissile(target, data.SetupData.Source, data.SetupData.Direction);
            _currentBounceRemain = bounceCount;
        }

        void SetupMissile(object target, Vector3 source, Vector3 direction)
        {
            var data = Get<MissileSetupActionData>(target);
            _direction = direction;
            Destination = source + _direction * FlyRange;
            Destination.y = Mathf.Min(Destination.y, limitHeight);
            Missile = data.Missile;
            Missile.Transform.rotation = Quaternion.LookRotation(_direction);
            Missile.Transform.position = source + Missile.Transform.rotation * startingOffset;
        }

        public override void OnHit(object target)
        {
        }
        
        public override void OnHitObstacle(object target)
        {
            var data = Get<MissileHitActionData>(target);
            FxHelper.SpawnFx(pools, data.HitPosition, data.HitDirection, hitFxPrefab, hitObstacleFeedback);
            _currentBounceRemain--;
            if (_currentBounceRemain <= 0)
            {
                Missile.Despawn();
                return;
            }

            _direction = Vector3.Reflect(_direction, data.HitNormal);
            Destination = data.HitPosition + _direction * FlyRange;
            Destination.y = Mathf.Min(Destination.y, limitHeight);
            Missile.Transform.rotation = Quaternion.LookRotation(_direction);
            Missile.Transform.position = data.HitPosition + Missile.Transform.rotation * startingOffset;

            //Missile.Despawn();
        }

        public override void OnMissileDeSpawn(object target)
        {
        }
    }
}