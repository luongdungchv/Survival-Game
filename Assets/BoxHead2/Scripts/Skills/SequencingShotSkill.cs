using System.Collections;
using System.Collections.Generic;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class SequencingShotSkill : RangedAutoChargeSkill
    {
        [SerializeField] float indicatorDuration;
        [SerializeField] int shootCount;
        [SerializeField] float intervalOffset;
        protected List<GameObject> _activeIndicators;
        protected List<ShootInputRecord> _shootInputRecords;
        protected int _shootCount;

        protected int shootIndex;

        protected Coroutine coroutineShowIndicator;

        public override bool IsStopConditionMet => !Actor.IsPlayingAnim && shootIndex >= _shootCount;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _shootCount = 0;
            shootIndex = 0;
            _activeIndicators ??= new List<GameObject>();
            _activeIndicators.Clear();

            _shootInputRecords ??= new List<ShootInputRecord>();
            _shootInputRecords.Clear();
        }
        

        public override void OnCustomEvent(int index)
        {
            base.OnCustomEvent(index);
            if (shootIndex < _shootCount)
            {
                Actor.PlayAnimation(releaseAnim, 0, releaseAnimSpeed, false, false);
            }
        }

        protected override void UpdateIndicator(Vector3 dir)
        {
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            RecordInput();
            _activeIndicators.Add(SpawnIndicator());
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            weapons[0].StopCharge();
        }

        protected virtual GameObject SpawnIndicator()
        {
            var indicator = pools.Spawn(shootIndicatorPrefab);
            indicator.GetComponent<MaterialModifyIndicator>().StartUpdate(indicatorDuration);
            return indicator;
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            coroutineShowIndicator = Actor.StartCoroutine(IEShowIndicators());
        }

        protected override void DoStop()
        {
            base.DoStop();
            if(coroutineShowIndicator != null)
                Actor.StopCoroutine(coroutineShowIndicator);
            foreach (var indicator in _activeIndicators)
            {
                if (indicator != null) pools.Despawn(indicator.gameObject);
            }
            _activeIndicators.Clear();
        }

        IEnumerator IEShowIndicators()
        {
            var interval = holdDuration / shootCount - intervalOffset;
            var wait = new WaitForSeconds(interval);
            for(int i = 0; i < shootCount; i++)
            {
                RecordInput();
                _activeIndicators.Add(SpawnIndicator());
                _shootCount = _activeIndicators.Count;
                yield return wait;
            }
        }

        void RecordInput()
        {
            var aimPos = GetAimedPosition();
            var aimDir = GetAimedDirection().normalized;
            var inputRecord = new ShootInputRecord()
            {
                aimPosition = aimPos,
                aimDirection = aimDir,
                launchPosition = weapons[0].transform.position,
                horizontalAngle = Actor.Transform.eulerAngles.y
            };
            _shootInputRecords.Add(inputRecord);
        }

        protected struct ShootInputRecord
        {
            public Vector3 aimDirection;
            public Vector3 aimPosition;
            public Vector3 launchPosition;
            public float horizontalAngle;
        }
    }
}