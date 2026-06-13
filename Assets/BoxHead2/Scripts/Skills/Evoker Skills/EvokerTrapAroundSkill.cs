using System;
using System.Collections;
using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Skills;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/Evoker Trap")]
    public class EvokerTrapAroundSkill : Skill
    {
        [SerializeField] protected string animation;
        [SerializeField] AnimationClip clip;
        [SerializeField] protected float animSpeed = 1;
        [SerializeField] protected bool animFade = true;

        [SerializeField] MeleeHitData[] hitsData;

        [SerializeField] TrapRoundInfo[] trapRoundsInfo;

        [SerializeField] bool useCoroutineForSpawn;
        [SerializeField] float trapSpawnInterval;

        [SerializeField] float trapTriggerDelay;
        //[SerializeField] int numberOfTraps, numberOfRounds;

        //[SerializeField] float roundOffset;
        [SerializeField] EvokerTrap trapPrefab;

        [SerializeField] LayerMaskType damageLayer;

        [SerializeField] float trackingSpeed;
        [SerializeField] Feedback startFeedback;
        [SerializeField] Vector3 offsetPosition;

        bool _moving;
        float _lastFrame;
        bool _tracking;

        MeleeHitData _hitData;
        CombinedDamageData[] _combinedDamageData;

        protected bool tracking;
        protected Vector3 aimDir;
        protected Vector3 aimPos;

        List<EvokerTrap>[] _activeTrapRounds;

        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _moving = false;
            _tracking = false;
            
            _combinedDamageData = new CombinedDamageData[hitsData.Length];
            for (int i = 0; i < hitsData.Length; i++)
            {
                SetupCombineDamage(i);
            }

            _activeTrapRounds ??= new List<EvokerTrap>[trapRoundsInfo.Length];
        }

        protected override void OnDetach()
        {
            base.OnDetach();
            CleanUpTraps();
        }

        protected override void DoStop()
        {
            base.DoStop();
            if (useCoroutineForSpawn) return;
            CleanUpTraps();
        }

        void CleanUpTraps()
        {
            if (_activeTrapRounds != null && _activeTrapRounds.Length > 0)
            {
                foreach (var trapRound in _activeTrapRounds)
                {
                    if(trapRound == null) continue;
                    foreach (var trap in trapRound)
                    {
                        if(trap == null) continue;
                        if (!trap.Triggered) pools.Despawn(trap.gameObject);
                    }
                    trapRound.Clear();
                }
            }
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

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            if (index < 0) return;
            if (index >= trapRoundsInfo.Length) return;

            var numberOfTraps = trapRoundsInfo[index].numberOfTraps;
            var roundOffset = trapRoundsInfo[index].trapsOffset;
            var trapsScale = trapRoundsInfo[index].trapsScale;
            var angleOffset = 360 / (float)numberOfTraps;
            
            _activeTrapRounds[index] ??= new List<EvokerTrap>();
            var round = _activeTrapRounds[index];
            round.Clear();

            for (int i = 0; i < numberOfTraps; i++)
            {
                var angle = i * angleOffset * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var worldDir = Actor.Transform.InverseTransformDirection(dir);
                worldDir *= roundOffset;

                var trap = pools.Spawn(trapPrefab);
                trap.Triggered = false;
                trap.Transform.position = Actor.Position + worldDir;
                trap.Transform.localScale = Vector3.one * trapsScale;
                trap.SetLayer(damageLayer);
                trap.SetOwner(Actor as Actor.Actor);
                trap.SetCombinedDamageData(_combinedDamageData[index]);

                round.Add(trap);
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if (index >= 0)
            {
                if (index >= trapRoundsInfo.Length) return;
                var round = _activeTrapRounds[index];
                foreach (var trap in round)
                {
                    trap.PlayAttackAnim();
                }
            }
            else
            {
                Actor.StartCoroutine(IESpawnTraps());
            }
        }

        IEnumerator IESpawnTraps()
        {
            for (int i = 0; i < trapRoundsInfo.Length; i++)
            {
                OnPrepareShoot(i);
                Actor.StartCoroutine(IEDelayTrigger(i));
                yield return new WaitForSeconds(trapSpawnInterval);
            }
        }

        IEnumerator IEDelayTrigger(int index)
        {
            yield return new WaitForSeconds(trapTriggerDelay);
            OnShoot(index);
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
            _combinedDamageData[index] = CombinedDamageData.Combine(GetDamageSourceData(), _hitData.DamageConfig);
        }

        [Serializable]
        struct TrapRoundInfo
        {
            public int numberOfTraps;
            public float trapsOffset;
            public float trapsScale;

            public TrapRoundInfo(int numberOfTraps, int trapsOffset, float trapsScale)
            {
                this.numberOfTraps = 0;
                this.trapsOffset = 0;
                this.trapsScale = trapsScale;
            }
        }
    }
}