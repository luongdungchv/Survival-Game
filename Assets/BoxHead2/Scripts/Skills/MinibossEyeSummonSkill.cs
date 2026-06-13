using Dacodelaac.Utils;
using DG.Tweening;
using UnityEngine;
using System.Collections;
namespace BoxHead2.Skills
{
    public class MinibossEyeSummonSkill : BasicAISummonSkill
    {
        [SerializeField] ParticleSystem fxPreSummonPrefab;
        [SerializeField] float fxPreSpawnDuration;

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if (index == -2)
            {
                foreach (var position in spawnPositions)
                {
                    var randomYAngle = Random.Range(0f, 360f);
                    var randomRotation = Quaternion.Euler(0, randomYAngle, 0);
                    //SpawnEnemy(position, randomRotation);
                    var fxPreSpawn = pools.Spawn(fxPreSummonPrefab);
                    fxPreSpawn.transform.position = Actor.Position;
                    fxPreSpawn.transform.DOMove(position, fxPreSpawnDuration).OnComplete(() =>
                    {
                        SpawnEnemy(position, randomRotation);
                        pools.Despawn(fxPreSpawn.gameObject);
                    });
                }

                summonFeedback?.Play();
            }
        }
    }
}