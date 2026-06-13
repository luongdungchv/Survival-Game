using BoxHead2.Actor;
using BoxHead2.Collection;
using BoxHead2.Helper;
using BoxHead2.Skills;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileFlyFollowAction")]
    public class MissileFlyFollowAction : MissileFlyAction
    {
        [SerializeField] float maxSpeed;
        [SerializeField] float acceleration = 2f;
        [SerializeField] float changeDirection = 1.5f;
        [SerializeField] DamageTakerCollection enemyCollection;
        [SerializeField] Feedback hitObstacleFeedback;
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] float delayFollow;
        [SerializeField] float startSpeed = 1f;
        [SerializeField] float changeY = -0.1f;

        Vector3 velocity;
        IDamageTaker shootTarget;
        float speed;
        Missile missile;
        float startTime;

        public override void Trigger(object target, IActor actor)
        {
            if (shootTarget == null || !shootTarget.CanBeTarget)
            {
                shootTarget = enemyCollection.GetNearest(out _, missile.Transform.position, 30, null);
            }
            
            Vector3 dir;
            if (Time.time - startTime >= delayFollow)
            {
                if (shootTarget != null)
                {
                    dir = shootTarget.LockPosition - missile.Transform.position;
                    if (dir.y >= 0) dir.y = changeY;
                }
                else
                {
                    dir = Random.rotation * Vector3.forward;
                    dir.y = changeY;
                }
            }
            else
            {
                dir = missile.transform.forward;
            }

            speed = Mathf.Min(maxSpeed, speed + Time.deltaTime * acceleration);
            velocity = missile.Transform.forward * speed;
            velocity += speed * changeDirection * Time.deltaTime * dir.normalized;
            velocity = velocity.normalized * speed;
            missile.Transform.rotation = Quaternion.LookRotation(velocity);
            missile.Transform.position += velocity * Time.deltaTime;
            missile.CurrentDirection = missile.Transform.forward;
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            var source = data.SetupData.Source;
            shootTarget = data.SetupData.Target;
            var dir = data.SetupData.Direction;
            dir.y = 0;
            dir.Normalize();

            missile = data.Missile;
            missile.Transform.position = source;
            missile.Transform.rotation = Quaternion.LookRotation(dir);
            speed = startSpeed;
            startTime = Time.time;
        }

        public override void OnHit(object target)
        {
        }

        public override void OnHitObstacle(object target)
        {
            var data = Get<MissileHitActionData>(target);
            FxHelper.SpawnFx(pools, data.HitPosition, data.HitDirection, hitFxPrefab, hitObstacleFeedback);
            missile.Despawn();
        }

        public override void OnMissileDeSpawn(object target)
        {
        }
    }
}