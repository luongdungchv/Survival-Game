using BoxHead2.Actor;
using BoxHead2.Helper;
using BoxHead2.Skills;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileFlyTrajectoryAction")]
    public class MissileFlyTrajectoryAction : MissileFlyAction
    {
        [SerializeField] float angle;
        [SerializeField] bool closerHigherAngle;
        [SerializeField] float gravity;
        [SerializeField] bool snapToTarget;
        [SerializeField] bool updateRotation = true;
        [SerializeField] Feedback hitObstacleFeedback;
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] bool isHitRotate = true;
        [SerializeField] Vector3 hitOffset;

        Vector3 velocity;
        Missile missile;

        Vector3 _defaultLaunchPos;

        public override void Trigger(object target, IActor actor)
        {
            velocity += gravity * Time.deltaTime * Vector3.down;
            missile.Transform.position += velocity * Time.deltaTime;
            if (updateRotation && velocity != Vector3.zero)
            {
                missile.Transform.rotation = Quaternion.LookRotation(velocity);
            }
            missile.CurrentDirection = missile.Transform.forward;
            if (missile.Transform.position.y <= _defaultLaunchPos.y - 20f)
            {
                missile.Despawn();
            }
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            var direction = data.SetupData.Direction;
            var source = data.SetupData.Source;
            var range = Mathf.Max(0.1f, data.SetupData.Range);
            var shootTarget = data.SetupData.Target;
            var destination = source + direction * range;
            if (snapToTarget)
            {
                if (shootTarget != null)
                {
                    destination = shootTarget.Position;
                }
                else
                {
                    destination = data.SetupData.TargetPos;
                }
            }
            //destination.y = 0;

            var r = destination - source;
            var l = r.magnitude;
            if (l > range)
            {
                l = range;
            }
            var a = Mathf.Min(89f, closerHigherAngle ? Mathf.Lerp(angle, 89, l/range) : angle) * Mathf.Deg2Rad;
            var g = gravity;
            var h = r.y;
            
            var cosA = Mathf.Cos(a);
            if (Mathf.Abs(cosA) < 0.001f)
            {
                cosA = Mathf.Sign(cosA) * 0.001f;
            }

            var k = Mathf.Max(0.001f, Mathf.Abs(Mathf.Sin(a) / cosA * l - h));
            var v = Mathf.Sqrt(g * l * l / (2 * k * cosA * cosA)); 
            r.y = l * (Mathf.Sin(a) / cosA);
            if (Mathf.Abs(r.y) > 1000f)
            {
                r.y = Mathf.Sign(r.y) * 1000f;
            }
            r.Normalize();
            velocity = r * v;
            
            missile = data.Missile;
            missile.Transform.position = source;
            _defaultLaunchPos = source;
            if (updateRotation && velocity != Vector3.zero)
            {
                missile.Transform.rotation = Quaternion.LookRotation(r);
            }

            var setupData = missile.SetupActionData.SetupData;
            setupData.TargetPos = destination;
            missile.SetupActionData.SetupData = setupData;
        }

        public override void OnHit(object target)
        {
        }
        
        public override void OnHitObstacle(object target)
        {
            var data = Get<MissileHitActionData>(target);
            FxHelper.SpawnFx(pools, data.HitPosition + hitOffset, isHitRotate ? data.HitDirection : Vector3.zero, hitFxPrefab, hitObstacleFeedback);
            missile.Despawn();
        }

        public override void OnMissileDeSpawn(object target)
        {
        }
    }
}