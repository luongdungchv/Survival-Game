using BoxHead2.Actor;
using BoxHead2.Helper;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileFlyStraightAction")]
    public class MissileFlyStraightAction : MissileFlyAction
    {
        [SerializeField] float speed;
        [SerializeField] float extendRange;
        [SerializeField] Vector3 startingOffset;
        [SerializeField] protected float limitHeight = 999;
        [SerializeField] protected Feedback hitObstacleFeedback;
        [SerializeField] protected ParticleSystem hitFxPrefab;
        protected Missile Missile;
        protected Vector3 Destination;
        
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
            Debug.LogError((0, actor));
            Missile.CurrentDirection = Missile.Transform.forward;
            if (SimpleMath.InRange(Missile.Transform.position, Destination, 0.1f))
            {
                Missile.Despawn();
            }
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            var direction = data.SetupData.Direction;
            var source = data.SetupData.Source;
            Destination = source + direction * FlyRange;
            Destination.y = Mathf.Min(Destination.y, limitHeight);
            Missile = data.Missile;
            Missile.Transform.rotation = Quaternion.LookRotation(direction);
            Missile.Transform.position = source + Missile.Transform.rotation * startingOffset;
        }

        public override void OnHit(object target)
        {
        }
        
        public override void OnHitObstacle(object target)
        {
            var data = Get<MissileHitActionData>(target);
            FxHelper.SpawnFx(pools, data.HitPosition, data.HitDirection, hitFxPrefab, hitObstacleFeedback);
            Missile.Despawn();
        }

        public override void OnMissileDeSpawn(object target)
        {
        }
    }
    
    public struct MissileSetupActionData
    {
        public Missile Missile { get; set; }
        public MissileSetupData SetupData { get; set; }

        public MissileSetupActionData(Missile missile, MissileSetupData setupData)
        {
            Missile = missile;
            SetupData = setupData;
        }
    }
}