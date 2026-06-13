using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Utils;
using Dacodelaac.Utils;
using DL.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Skills
{
    public class BasicAISummonSkill : Skill
    {
        [SerializeField] protected string animation;
        [SerializeField] protected int count;
        [SerializeField] protected float summonRadius;
        [SerializeField] protected AIConfig aiConfig;
        [SerializeField] protected AIConfig[] aiConfigList;
        [SerializeField] protected Feedback summonFeedback;
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;

        protected Vector3[] spawnPositions;

        bool _tracking;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            GetSpawnPositions();
        }
        

        protected override IEnumerator IEPerform()
        {
            Actor.PlayAnimation(animation, 0, 1, false, false);
            while (true)
            {   
                if (_tracking)
                {
                    var aimDir = GetAimedDirection();
                    Actor.RotateDirection(aimDir, 999f, true);
                }
                yield return null;
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if (index == -1)
            {
                foreach (var position in spawnPositions)
                {
                    var randomYAngle = Random.Range(0f, 360f);
                    var randomRotation = Quaternion.Euler(0, randomYAngle, 0);
                    SpawnEnemy(position, randomRotation);
                }

                summonFeedback?.Play();
            }
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            _tracking = true;
        }

        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            _tracking = false;
        }

        protected virtual void GetSpawnPositions()
        {
            spawnPositions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                var pos = Actor.Position + SimpleMath.RandomInCircleXZ() * summonRadius;
                if(NavMesh.SamplePosition(pos, out var hit, 100f, NavMesh.AllAreas))
                    pos = hit.position;
                spawnPositions[i] = pos;
            }
        }
        protected virtual void SpawnEnemy(Vector3 pos, Quaternion rotation)
        {
            var zoneIndex = -1;
            aiConfigList.GetRandomElement().SpawnAsync(Actor.TeamConfig, pos, rotation, 0,
                false, true, false, zoneIndex, enemy =>
                {
                    
                });
        }
    }
}