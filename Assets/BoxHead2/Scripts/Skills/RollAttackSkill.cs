using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/Roll Attack")]
    public class RollAttackSkill : Skill
    {
        [Header("Roll")]
        [SerializeField] string beginRollAnimation;
        [SerializeField] string endRollAnimation;
        [SerializeField] float prepareDuration;
        [SerializeField] float duration;
        [SerializeField] float moveSpeed;
        [SerializeField] float startSpeed;
        [SerializeField] float acceleration = 0f;
        [SerializeField] float rotateSpeed;
        [SerializeField] private float rotateSpeedAcceleration;
        [Header("Damage")]
        [SerializeField] float delayDamage;
        [SerializeField] DamageConfig damageConfig;
        [SerializeField] bool selfDamage;
        [SerializeField] bool wallKnockBack;
        [SerializeField] private SubAction wallKnockBackAction;
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] LayerMaskType friendLayer = LayerMaskType.HitBoxLayer;
        [SerializeField] LayerMask wallLayer;
        [SerializeField] bool friendlyFire;
        [SerializeField] bool dieOnStop;
        [SerializeField] private bool moveNextSkillAfterWallKnock;
        [SerializeField] private bool dieOnMaxSpeed;
        [SerializeField] DamageConfig friendlyFireDamageConfig;
        [Header("HurtBox")]
        [SerializeField] float hurtBoxResetInterval = -1;
        [SerializeField] HurtBoxGroup[] hurtBoxGroupPrefab;
        [Header("Feedback")]
        [SerializeField] bool spawnFxOnPrepare = true;
        [SerializeField] bool despawnFxImmediately = true;
        [SerializeField] private bool stopWhenHitEnemy;
        [SerializeField] Feedback prepareFeedback;
        [SerializeField] ParticleSystem rollFxPrefab;
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] HitFeedback hitFeedback;
        [SerializeField] Feedback[] feedbacks;
        
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        public virtual float Duration => duration;
        HurtBoxGroup[] hurtBoxGroups;
        HurtBox[] hurtBoxes;
        ParticleSystem rollFx;
        float lastTimeClearHurtBox;
        bool shouldClearHurtBox;
        CombinedDamageData combinedDamageData;
        CombinedDamageData friendlyFireCombinedDamageData;
        IDamageTaker damageTaker;
        private float curSpeed;
        
        protected override void PrepareSkill()
        {
            Actor.StopMovement();
            damageTaker = Actor as IDamageTaker;
            var damageSourceData = GetDamageSourceData();
            combinedDamageData = CombinedDamageData.Combine(damageSourceData, damageConfig);
            friendlyFireCombinedDamageData = CombinedDamageData.Combine(damageSourceData, friendlyFireDamageConfig);
            HurtBoxHelper.SetupHurtBoxes(pools, Actor, hurtBoxGroupPrefab, out hurtBoxGroups, out hurtBoxes);
            
            curSpeed = acceleration == 0f ? moveSpeed : startSpeed;
        }

        protected override void DoStop()
        {
            base.DoStop();
            DespawnRollFx();
            HurtBoxHelper.DespawnHurtBoxes(pools, ref hurtBoxGroups);
        }

        protected override IEnumerator IEPerform()
        {
            Actor.OnSkillBeginAttack();
            Actor.PlayAnimation(beginRollAnimation, 0, 1, true, false);
            if (prepareFeedback)
            {
                prepareFeedback.Play();
            }

            if (spawnFxOnPrepare)
            {
                SpawnRollFx();
            }

            var t = prepareDuration;
            while (t > 0)
            {
                t -= Time.deltaTime;
                Actor.RotateDirection(Actor.AimedDirection, Actor.RotateSpeed);
                yield return null;
            }
            
            if (!spawnFxOnPrepare)
            {
                SpawnRollFx();
            }
            
            t = Duration;
            var dir = Actor.Transform.forward;
            var d = delayDamage;
            curSpeed = acceleration == 0f ? moveSpeed : startSpeed;
            
            var curRotateSpeed = rotateSpeed;
            while (t > 0)
            {
                t -= Time.deltaTime;
                dir += curRotateSpeed * Time.deltaTime * Actor.AimedDirection;
                
                curRotateSpeed = Mathf.Clamp(curRotateSpeed + rotateSpeedAcceleration * Time.deltaTime, 0f, 999f);
                
                dir.Normalize();
                
                if (curSpeed < moveSpeed)
                {
                    curSpeed += acceleration * Time.deltaTime;
                }
                
                Actor.RotateDirection(dir, 0, true);
                Actor.MoveDirection(Actor.Transform.forward, curSpeed);
                
                shouldClearHurtBox = hurtBoxResetInterval > 0 && Time.time - lastTimeClearHurtBox > hurtBoxResetInterval;
                if (shouldClearHurtBox)
                {
                    lastTimeClearHurtBox = Time.time;
                }

                if (d > 0)
                {
                    d -= Time.deltaTime;
                }
                else
                {
                    ExecuteHurtBox();
                    if (friendlyFire)
                    {
                        ExecuteHurtBoxFriendlyFire();
                    }
                    if (wallKnockBack)
                    {
                        ExecuteHurtBoxWall();
                    }
                }
                yield return null;
            }

            DespawnRollFx();
            if (dieOnStop)
            {
                Actor.InstantDead(new CombinedDamageForceData());
            }
            else
            {
                if (!string.IsNullOrEmpty(endRollAnimation))
                {
                    Actor.PlayAnimation(endRollAnimation, 0, 1, true, false);
                }
            }
            
            Actor.OnSkillEndAttack();
            Actor.OnSkillCanMoveNextSkill();
        }

        void ExecuteHurtBox()
        {
            var hit = false;
            HurtBoxHelper.ExecuteManualHurtBox(Actor.Position, Actor.Transform.forward, Actor.TeamConfig.GetLayerMask(damageLayer), combinedDamageData,
                shouldClearHurtBox, hurtBoxes, (pos, dir, hitType, damageHitTaker) =>
                {
                    hit = true;
                    FxHelper.SpawnFx(pools, pos, dir, hitFxPrefab, null);
                    hitFeedback.Play(hitType);
                });
            if (hit && selfDamage)
            {
                SelfDamage();
            }

            if (hit && stopWhenHitEnemy)
            {
                if (dieOnMaxSpeed && curSpeed >= moveSpeed)
                {
                    damageTaker.InstantDead(friendlyFireCombinedDamageData.DamageForce);
                }
                else
                {
                    Actor.OnSkillCanMoveNextSkill();
                }
            }
        }

        void ExecuteHurtBoxWall()
        {
            if (HurtBoxHelper.ExecuteManualHurtBoxCheck(wallLayer, hurtBoxes))
            {
                SelfDamage();
                if (wallKnockBackAction != null)
                {
                    wallKnockBackAction.Trigger(this, Actor);
                }
                
                if (dieOnMaxSpeed && curSpeed >= moveSpeed)
                {
                    damageTaker.InstantDead(friendlyFireCombinedDamageData.DamageForce);
                }

                if (moveNextSkillAfterWallKnock)
                {
                    Actor.OnSkillCanMoveNextSkill();
                }
            }
        }

        void ExecuteHurtBoxFriendlyFire()
        {
            var hit = false;
            HurtBoxHelper.ExecuteManualHurtBox(Actor.Position, Actor.Transform.forward, Actor.TeamConfig.GetLayerMask(friendLayer), friendlyFireCombinedDamageData, 
                shouldClearHurtBox, hurtBoxes,
                (pos, dir, hitType, damageHitTaker) =>
                {
                    hit = true;
                    FxHelper.SpawnFx(pools, pos, dir, hitFxPrefab, null);
                    hitFeedback.Play(hitType);
                }, damageTaker);
            if (hit && selfDamage)
            {
                SelfDamage();
            }
        }

        void SelfDamage()
        {
            var selfDamageData = friendlyFireCombinedDamageData;
            selfDamageData.UpdateDamageForce( -Actor.Transform.forward, Actor.LockPosition, Actor.LockPosition);
            if (dieOnStop)
            {
                damageTaker.InstantDead(selfDamageData.DamageForce);
            }
            else
            {
                damageTaker.TakeDamage(selfDamageData);
            }
        }

        void SpawnRollFx()
        {
            if (rollFxPrefab)
            {
                rollFx = pools.Spawn(rollFxPrefab, Actor.Transform);
                var t1 = rollFxPrefab.transform;
                var t2 = rollFx.transform;
                t2.localPosition = t1.localPosition;
                t2.localRotation = t1.localRotation;
                rollFx.Play();
            }
        }

        void DespawnRollFx()
        {
            if (rollFx)
            {
                rollFx.Stop();
                if (despawnFxImmediately)
                {
                    pools.Despawn(rollFx.gameObject);
                    rollFx = null;
                }
                else
                {
                    rollFx.transform.parent = null;
                    rollFx = null;
                }
            }
        }

        public override void OnFeedbackEvent(int index)
        {
            base.OnFeedbackEvent(index);
            if (feedbacks.Length > index)
            {
                feedbacks[index].Play();
            }
        }
    }
}