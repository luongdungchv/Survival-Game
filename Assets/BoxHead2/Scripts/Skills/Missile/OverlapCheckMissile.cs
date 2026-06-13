using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class OverlapCheckMissile : Missile
    {
        [SerializeField] int bufferSize;

        Collider[] overlapBuffer;

        public override void Initialize()
        {
            base.Initialize();
            overlapBuffer = new Collider[bufferSize];
        }

        protected override void RaycastCheck()
        {
            if (overlapBuffer == null || overlapBuffer.Length == 0) return;
            var count = Physics.OverlapSphereNonAlloc(transform.position, _radiusMissile, overlapBuffer, Actor.TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer) | obstacleMask);
            for (var i = 0; i < count; i++)
            {
                if(i >= bufferSize) continue;
                var collider = overlapBuffer[i];
                var damageTaker = collider.GetComponentInParent<IDamageTaker>();
                var point = collider.transform.position;
                if (damageTaker is { Alive: true })
                {
                    OnHit(damageTaker, collider.transform.position);
                    flyFeedback?.Stop();
                }
                else if (damageTaker == null)
                {
                    if (!ignoreObstacle && Shooted && !Despawned && flyAction)
                    {
                        missileHitActionData = new MissileHitActionData(null, point, CurrentDirection, Vector3.zero, SourceData);
                        flyAction.OnHitObstacle(this);
                        flyFeedback?.Stop();
                    }
                }
            }
            
        }
    }
}