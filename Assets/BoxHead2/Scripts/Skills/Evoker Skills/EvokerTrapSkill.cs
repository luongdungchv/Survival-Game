using System.Collections;
using System.Collections.Generic;
using BoxHead2.Combat;
using BoxHead2.Skills;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/Evoker Trap")]
    public class EvokerTrapSkill : Skill
    {
        [SerializeField] protected string animation;
        [SerializeField] AnimationClip clip;
        [SerializeField] protected float animSpeed = 1;
        [SerializeField] protected bool animFade = true;
        
        [SerializeField] int numberOfTraps;
        [SerializeField] float trapSpawnInterval;
        [SerializeField] float trapSpeedMultiplier;
        [SerializeField] float trapGap;
        [SerializeField] EvokerTrap trapPrefab;

        [SerializeField] LayerMaskType damageLayer;
        [SerializeField] MeleeHitData[] hitsData;

        [SerializeField] float trackingSpeed;
        [SerializeField] Feedback startFeedback;
        [SerializeField] Vector3 offsetPosition;
        

        MeleeHitData _hitData;
        CombinedDamageData _combinedDamageData;
        
        protected bool tracking;
        protected Vector3 aimDir;
        protected Vector3 aimPos;

        List<EvokerTrap> _activeTraps;
        
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _activeTraps ??= new List<EvokerTrap>();
            _activeTraps.Clear();
            SetupCombineDamage(0);
        }

        protected override void DoStop()
        {
            base.DoStop();
            if (_activeTraps != null && _activeTraps.Count > 0)
            {
                foreach (var trap in _activeTraps)
                {
                    if(!trap) continue;
                    if(!trap.Triggered) pools.Despawn(trap.gameObject);
                }
            }

            _activeTraps.Clear();
        }

        protected override IEnumerator IEPerform()
        {
            Actor.PlayAnimation(animation, 0, 1, animFade, false);
            Actor.SetRootMotionMult(rootMotionMult);
            if (startFeedback)
            {
                startFeedback.Play();
            }
            while (true)
            {
                if (tracking)
                {
                    aimDir = GetAimedDirection();
                    aimPos = GetAimedPosition();
                    Actor.RotateDirection(aimDir, trackingSpeed, true);
                }

                yield return null;
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if (_activeTraps == null || _activeTraps.Count <= index) return;
            var trap = _activeTraps[index];
            trap.PlayAttackAnim();
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            
            var castDir = Actor.ForwardDirection;
            var actorPos = Actor.Position;
            
            var trap = pools.Spawn(trapPrefab);
            trap.Triggered = false;
            trap.transform.position = actorPos + castDir * (index * trapGap) + offsetPosition;
            trap.transform.localScale = Vector3.one;
            trap.SetLayer(damageLayer);
            trap.SetOwner(Actor as Actor.Actor);
            trap.SetCombinedDamageData(_combinedDamageData);
            
            _activeTraps.Add(trap);
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            tracking = true;
            aimDir = GetAimedDirection();
            aimPos = GetAimedPosition();
        }
        
        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            tracking = false;
            aimDir = GetAimedDirection();
            aimPos = GetAimedPosition();
        }

        void SetupCombineDamage(int index)
        {
            if (hitsData.Length == 0)
            {
                return;
            }
            index = Mathf.Clamp(index, 0, hitsData.Length);
            _hitData = hitsData[index];
            _combinedDamageData = CombinedDamageData.Combine(GetDamageSourceData(), _hitData.DamageConfig);
        }
        
        
    }
}