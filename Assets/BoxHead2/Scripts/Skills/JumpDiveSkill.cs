using System.Collections;
using System.Linq;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using Dacodelaac.DebugUtils;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/Jump Dive")]
    public class JumpDiveSkill : Skill
    {
        [Header("Animation")]
        [SerializeField] protected string animation;
        [SerializeField] AnimationClip clip;
        [SerializeField] float animSpeed = 1;
        [SerializeField] bool canTracking = true;
        [Header("Weapon")]
        [SerializeField] bool useCurrentWeapon;
        [SerializeField] AttachConfig[] attachConfigs;
        [Header("Explode")]
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] DamageConfig damageConfig;
        [Header("Feedback")]
        [SerializeField] Feedback beginFeedback;
        [SerializeField] bool scaleFx = true;
        [SerializeField] AttackIndicatorData indicatorData;
        [SerializeField] ParticleSystem explosionFxPrefab;
        [SerializeField] Feedback explosionFeedback;
        
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;

        bool tracking;
        bool moving;
        
        protected Vector3 targetPos;
        protected Vector3 targetDirection;
        
        protected float moveTime;
        float moveSpeed;
        Weapon[] weapons;
        AttackIndicator indicator;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            if (useCurrentWeapon)
            {
                weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
            }
            else
            {
                weapons = SkillHelper.AttachWeapons(pools, Actor, null, attachConfigs, false);
            }
            var beginMoveEvent = clip.events.FirstOrDefault(e => e.functionName == "OnBeginMove");
            var stopMoveEvent = clip.events.FirstOrDefault(e => e.functionName == "OnStopMove");
            if (beginMoveEvent != null && stopMoveEvent != null)
            {
                moveTime = (stopMoveEvent.time - beginMoveEvent.time) / animSpeed;
            }
            UpdateTargetPosition();
            tracking = false;
            moving = false;
            Actor.DisableMovement();
            if (indicatorData != null) indicator = indicatorData.Spawn(pools);
        }
        
        protected override void DoStop()
        {
            base.DoStop();
            Actor.EnableMovement();
            SkillHelper.StopWeapon(weapons);
            if (!useCurrentWeapon)
            {
                SkillHelper.DetachWeapons(pools, weapons, false);
            }
            weapons = null;
            DespawnIndicator();
        }

        protected override IEnumerator IEPerform()
        {
#if UNITY_EDITOR
            Dacoder.DrawCircle(Actor.Position, Range, Color.red, 1f);
#endif

            Actor.PlayAnimation(animation, 0, animSpeed, true, false);
            Actor.SetRootMotionMult(rootMotionMult);
            if (beginFeedback)
            {
                beginFeedback.Play();
            }

            while (true)
            {
                if (tracking)
                {
                    UpdateTargetPosition();
                }

                Actor.RotateDirection(targetDirection, Actor.RotateSpeed);

                if (moving && moveSpeed > 0)
                {
                    Actor.Transform.position = Vector3.MoveTowards(Actor.Transform.position, targetPos,
                        moveSpeed * Time.deltaTime);
                }

                UpdateIndicator();

                yield return null;
            }
        }
        public override void OnBeginMove(int index)
        {
            base.OnBeginMove(index);
            if (NavMesh.SamplePosition(targetPos, out var hit, 30f, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                targetPos = hit.position;
            }
            else
            {
                targetPos = Actor.Position;
            }
            moveSpeed = (targetPos - Actor.Position).magnitude / moveTime;
            moving = true;
            Actor.DisableMovement();
        }
        
        public override void OnStopMove(int index)
        {
            base.OnStopMove(index);
            moving = false;
            Actor.EnableMovement();
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            tracking = canTracking;
        }

        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            tracking = false;
            if (canTracking)
            {
                UpdateTargetPosition();
            }
        }
        
        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            SkillHelper.ToggleTrail(weapons, true);
        }

        public override void OnStopTrail(int index)
        {
            base.OnStopTrail(index);
            SkillHelper.ToggleTrail(weapons, false);
        }

        public override void OnCustomEvent(int index)
        {
            base.OnCustomEvent(index);
            var pos = Actor.Transform.TransformPoint(explosionFxPrefab.transform.position);
            var dir = Actor.Transform.TransformDirection(explosionFxPrefab.transform.forward);
            FxHelper.SpawnExplodeFx(pools, pos, dir, Radius, scaleFx, explosionFxPrefab, explosionFeedback, Actor);
            ExplodeHelper.Scan(pos, Radius, CombinedDamageData.Combine(GetDamageSourceData(), damageConfig), Actor.TeamConfig.GetLayerMask(damageLayer), null);
            DespawnIndicator();
        }

        void UpdateTargetPosition()
        {
            targetPos = GetAimedPosition() - Actor.Transform.rotation * explosionFxPrefab.transform.position;
            targetDirection = GetAimedDirection();
            Debug.DrawLine(Actor.Position, targetPos);
        }

        protected virtual void UpdateIndicator()
        {
            if (!indicator)
            {
                return;
            }
            indicator.DoUpdate(Actor.Transform, targetPos, targetDirection, Range, Radius);
        }

        void DespawnIndicator()
        {
            if (indicator)
            {
                pools.Despawn(indicator.gameObject);
                indicator = null;
            }
        }
    }
}