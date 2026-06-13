using System;
using System.Collections;
using BoxHead2.Helper;
using BoxHead2.Items;
using Dacodelaac.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoxHead2.Skills
{
    public class IronMaidenSummonSkill : BasicAISummonSkill
    {
        [SerializeField] ParticleSystem fxPreSummonPrefab;
        [SerializeField] ParticleSystem fxOpenPrefab, fxPostSummonPrefab;
        [SerializeField] float fxPreSpawnDuration, summonInterval;
        [SerializeField] AttachConfig[] attachConfigs;
        [SerializeField] float maxAngle;
        [SerializeField] float minSummonRange;

        Weapon[] weapons;

        protected override void PrepareSkill()
        {
            weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
            Actor.RotateDirection(GetAimedDirection(), 999, true);
            base.PrepareSkill();
        }

        protected override void GetSpawnPositions()
        {
            base.GetSpawnPositions();
            spawnPositions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                var randomRotation = Quaternion.Euler(0, Random.Range(-maxAngle, maxAngle), 0);
                var randomDir = randomRotation * Actor.Transform.forward;
                randomDir.Normalize();
                randomDir *= Random.Range(minSummonRange, summonRadius);
                
                spawnPositions[i] = weapons[0].LauncherPosition.Set(y: Actor.Position.y) + randomDir;
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            GetSpawnPositions();
            if (index == -2)
            {
                Actor.StartCoroutine(IESummon());

                summonFeedback?.Play();
            }
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            var fxOpen = pools.Spawn(fxOpenPrefab);
            fxOpen.transform.SetParent(Actor.Transform);
            fxOpen.transform.localPosition = fxOpenPrefab.transform.localPosition;
            fxOpen.transform.localRotation = fxOpenPrefab.transform.localRotation;
            fxOpen.Play();
        }

        IEnumerator IESummon()
        {
            foreach (var position in spawnPositions)
            {
                var randomYAngle = Random.Range(0f, 360f);
                var randomRotation = Quaternion.Euler(0, randomYAngle, 0);
                var fxPreSpawn = pools.Spawn(fxPreSummonPrefab);
                fxPreSpawn.transform.position = weapons[0].LauncherPosition;
                Actor.StartCoroutine(IEAnimateSummon(fxPreSpawn.transform, weapons[0].LauncherPosition, position, () =>
                {
                    SpawnEnemy(position, randomRotation);
                    pools.Despawn(fxPreSpawn.gameObject);
                    
                    var fxPostSpawn = pools.Spawn(fxPostSummonPrefab);
                    fxPostSpawn.transform.position = position;
                }));
                yield return new WaitForSeconds(summonInterval);
            }
        }

        IEnumerator IEAnimateSummon(Transform fx, Vector3 start, Vector3 end, Action onComplete)
        {
            var t = 0f;
            var randomVector = Random.onUnitSphere;
            var dir = end - start;
            var length = dir.magnitude;
            var upVector = Vector3.Cross(dir.normalized, randomVector);
            if(upVector.y < 0) upVector.y = -upVector.y;
            upVector.Normalize();
            var randomOffset = Random.Range(length / 10f, length);
            while (t < 1)
            {
                t += Time.deltaTime / fxPreSpawnDuration;
                fx.transform.position = SimpleMath.CircularInterpolate(start, end, t, upVector, randomOffset);
                yield return null;
            }
            onComplete?.Invoke();
        }
    }
}