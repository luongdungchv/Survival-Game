using System;
using System.Collections.Generic;
using BoxHead2.Combat;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public class AreaOfEffectWall : BaseMono
    {
        [SerializeField] LayerMask ground;
        [SerializeField] AreaOfEffect areaOfEffectPrefab;
        [SerializeField] GameObject indicatorPrefab;

        Transform followTarget;
        Vector3 lastPos;
        AreaOfEffectData areaOfEffectData;
        DamageSourceData sourceData;
        Func<bool> stopCondition;
        float Distance => areaOfEffectData.Radius * 2;
        List<GameObject> listIndicators = new();
        LayerMask damageLayer;

        public void Setup(AreaOfEffectData areaOfEffectData, DamageSourceData sourceData, Transform followTarget, LayerMask damageLayer, Func<bool> stopCondition, float range, Vector3 direction)
        {
            this.areaOfEffectData = areaOfEffectData;
            this.sourceData = sourceData;
            this.followTarget = followTarget;
            this.stopCondition = stopCondition;
            this.damageLayer = damageLayer;
            lastPos = followTarget.position;
            
            listIndicators.Clear();  
            SetupSpawnIndicator(direction, range);
            // Spawn(lastPos, followTarget.forward);
        }

        public override void Tick()
        {
            base.Tick();

            if (stopCondition.Invoke())
            {
                Despawn();
                return;
            }
            
            var dir = followTarget.position - lastPos;
            dir.y = 0;
            var distance = dir.magnitude;
            while (distance >= Distance)
            {
                distance -= Distance;
                var pos = lastPos + Distance * dir.normalized;
                Spawn(pos, dir);
                lastPos = pos;
                if (listIndicators.Count > 0)
                {
                    var indicator = listIndicators[0];
                    listIndicators.Remove(indicator);
                    pools.Despawn(indicator);
                }
            }
        }

        void Spawn(Vector3 pos, Vector3 dir)
        {
            pos.y = 0;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down * 1.2f, 2, ground))
            {
                var areaOfEffect = pools.Spawn(areaOfEffectPrefab);
                areaOfEffect.Transform.position = pos;
                areaOfEffect.Setup(null, areaOfEffectData, sourceData, damageLayer, null);
            }
        }

        void SetupSpawnIndicator(Vector3 dir, float range)
        {
            var lastPosIndicator = lastPos;
            while (range > Distance)
            {
                range -= Distance;
                var pos = lastPosIndicator + Distance * dir.normalized;
                SpawnIndicator(pos, dir);
                lastPosIndicator = pos;
            }
        }

        void SpawnIndicator(Vector3 pos, Vector3 dir)
        {
            pos.y = 0;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down * 1.2f, 2, ground))
            {
                var indicator = pools.Spawn(indicatorPrefab);
                indicator.transform.position = pos + Vector3.up * 0.1f;
                indicator.transform.localScale = Vector3.one * areaOfEffectData.Radius;
                listIndicators.Add(indicator);
            }
        }

        void Despawn()
        {
            if (listIndicators.Count > 0)
            {
                while (listIndicators.Count > 0)
                {
                    var indicator = listIndicators[0];
                    listIndicators.Remove(indicator);
                    pools.Despawn(indicator);
                }
            }
            pools.Despawn(gameObject);
        }
    }
}