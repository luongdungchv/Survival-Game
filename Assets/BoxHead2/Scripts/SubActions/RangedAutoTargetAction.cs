using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoxHead2.Actor;
using BoxHead2.Helper;
using BoxHead2.Skills;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/RangedAutoTargetAction")]
    public class RangedAutoTargetAction : SubAction
    {
        [Header("Shoot")]
        [SerializeField] Vector3 launchPositionOffset;
        [SerializeField] ParticleSystem launcherFxPrefab;
        [SerializeField] float delay;
        [SerializeField] float range;
        [SerializeField] float randomRange;
        [SerializeField] int shootTime = 1;
        [SerializeField] float shootTimeDelay = 0.1f;
        [SerializeField] float shootTimeSpread = 5f;
        [SerializeField] float spreadAngleStep = 0;
        [SerializeField] float randomAngleStep = 0;
        [SerializeField] int spreadCount = 1;
        [SerializeField] int randomSpreadCount;
        [SerializeField] bool isGetNearestTarget = false;
        [Header("Missile")]
        [SerializeField] MissileData missileData;
        [Header("Feedback")]
        [SerializeField] Feedback shootFeedback;

        RangedAttackActionData data;
        IActor actor;

        public override void Trigger(object target, IActor actor)
        {
            this.actor = actor;
            data = Get<RangedAttackActionData>(target);
            StartPerformRoutine();
        }

        protected override IEnumerator IEPerform()
        {
            var except = new HashSet<IDamageTaker>();
            // var enemy = stickMan.AimedEnemy != null && StickManType.Enemy.HasFlag(stickMan.AimedEnemy.Type) ? stickMan.AimedEnemy : null;
            var enemy = isGetNearestTarget ? actor.GetNearestEnemy(range) : actor.GetEnemyInRadius(actor.Position, range).FirstOrDefault();
            var pos = actor.Position;
            var launchPos = pos + actor.Transform.rotation * launchPositionOffset;
            if (launcherFxPrefab)
            {
                var launcher = pools.Spawn(launcherFxPrefab);
                launcher.transform.position = launchPos;
                launcher.Play();
            }

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            for (var i = 0; i < shootTime; i++)
            {
                // find target
                if (enemy == null || !enemy.CanBeTarget)
                {
                    enemy = actor.GetNearestEnemy(30, except);
                }
                // calculate direction
                Vector3 dir;
                Vector3 targetPos;
                if (enemy != null && enemy.CanBeTarget)
                {
                    dir = enemy.LockPosition - actor.LockPosition;
                    var mag = dir.magnitude;
                    dir.Normalize();
                    if (dir == Vector3.zero)
                    {
                        dir = actor.AimedDirection;
                    }
                    if (mag > range)
                    {
                        targetPos = actor.LockPosition + dir * range;
                    }
                    else
                    {
                        targetPos = enemy.LockPosition;
                    }
                    //targetPos.y = 0;
                }
                else
                {
                    var r = Random.Range(0f, 360f);
                    dir = Quaternion.Euler(0, r, 0) * Vector3.forward;
                    targetPos = actor.Position + Random.Range(0.5f, 1f) * range * dir;
                    //targetPos.y = 0;
                }

                SkillHelper.OnShoot(pools, new ShootData(actor, data.SourceData, missileData, null,
                    pos, launchPos, dir, false, enemy, targetPos,
                    1, shootTimeDelay, shootTimeSpread, 0, 0,
                    spreadAngleStep, randomAngleStep, spreadCount, randomSpreadCount, 0, 
                    range, randomRange, 0, true, false, shootFeedback, null, false, null));
                
                if (enemy != null)
                {
                    except.Add(enemy);
                }
                enemy = null;

                yield return new WaitForSeconds(0.1f);
            }
        }

        public override void Detach(object target)
        {
            StopPerformRoutine();
        }

        public override void Attach(object target)
        {
            StopPerformRoutine();
        }
    }
}