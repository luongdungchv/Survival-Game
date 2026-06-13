using Dacodelaac.Utils;
using DG.DemiLib;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class BasicRandomMultiShotSkill : SequencingShotSkill
    {
        [SerializeField] string releaseAnimAfter;
        [SerializeField] LayerMask shootMask;
        [SerializeField] float randomWidth;

        Vector3 cachedTarget;


        public override void OnBeginTracking(int index)
        {
            // base.OnBeginTracking(index);
            tracking = false;
            UseCachedInput = false;
            var aimDir = Actor.AimedEnemy.Position - Actor.Position;
            aimDir.y = 0;
            aimDir.Normalize();
            Actor.RotateDirection(aimDir, 999, true);
        }

        public override void OnStopTracking(int index)
        {
            tracking = false;
            UseCachedInput = false;
            cachedTarget = Actor.GetAimPosition();
            UseCachedInput = true;
            //cachedTarget.y = weapons[0].LauncherPosition.y;
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            UseCachedInput = false;
        }

        protected override GameObject SpawnIndicator()
        {
            var indicator = base.SpawnIndicator();
            SetIndicatorProps(indicator.GetComponent<MaterialModifyIndicator>(), _shootInputRecords.Count - 1);
            return indicator;
        }


        public override void OnCustomEvent(int index)
        {
            base.OnCustomEvent(index);
            if (shootIndex < _shootCount)
            {
                Actor.PlayAnimation(releaseAnimAfter, 0, releaseAnimSpeed, false, false);
            }
        }

        void SetIndicatorProps(MaterialModifyIndicator indicator, int index)
        {
            var shootRecord = _shootInputRecords[index];
            indicator.transform.position = shootRecord.launchPosition;
            var aimDir = (shootRecord.aimPosition - shootRecord.launchPosition).normalized;
            indicator.transform.rotation = Quaternion.FromToRotation(Vector3.forward, aimDir);
                
            var startPos = weapons[0].LauncherPosition;
            if (Physics.Raycast(startPos, shootRecord.aimDirection, out var hit, 100, shootMask))
            {
                var length = Vector3.Distance(startPos, hit.point);
                indicator.transform.localScale = indicator.transform.localScale.Set(z: length);
            }
            else
            {
                indicator.transform.localScale = indicator.transform.localScale.Set(z: Range);
            }
        }

        public override void OnShoot(int index)
        {
            UseCachedInput = true;
            if (shootIndex >= _shootCount) return;
            var shootRecord = _shootInputRecords[shootIndex];
            aimDir = shootRecord.aimDirection;
            aimPos = shootRecord.aimPosition;
            
            CachedAimedEnemy = null;
            pools.Despawn(_activeIndicators[shootIndex].gameObject);
            base.OnShoot(index);
            shootIndex++;
            UseCachedInput = false;
        }
        
        

        protected override Vector3 GetAimedPosition()
        {
            if (tracking) return base.GetAimedPosition();
            var aimDir = cachedTarget - weapons[0].LauncherPosition;
            aimDir.Normalize();
            var offsetDir = Vector3.Cross(Vector3.up, aimDir).normalized;
            offsetDir *= Random.Range(-randomWidth, randomWidth);
            offsetDir.y = 0;
            var newTarget = cachedTarget + offsetDir;
            CachedAimedDirection = newTarget - weapons[0].LauncherPosition;
            CachedAimedDirection.Normalize();
            CachedAimedPosition = newTarget;
            return newTarget;
        }
    }
}