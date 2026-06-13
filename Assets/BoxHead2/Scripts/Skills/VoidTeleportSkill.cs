using System.Collections;
using Dacodelaac.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace BoxHead2.Skills
{
    public class VoidTeleportSkill : Skill
    {
        [SerializeField] float radiusPos;
        [SerializeField] string animation;
        [FormerlySerializedAs("teleportEffect")] [SerializeField] ParticleSystem fxStartTeleport;
        [SerializeField] ParticleSystem fxPreTeleport, fxDestination, fxAppear;
        [SerializeField] float delayTime = 0.75f;
        [SerializeField] float delayAppear = 0.75f, delayPrepareDestination = 0.75f, delayMoveNextSkill = 0.25f;
        [SerializeField] TeleportType teleportType;
        [SerializeField] float offset = 1;
        [SerializeField] bool prepareDestinationOnStart;
        [SerializeField] Feedback[] feedbacks;
        public override bool IsStopConditionMet => !_isPlaySkill;
        bool _isPlaySkill;
        
        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            Actor.TurnOffHitBox();
            Actor.DisableMovement();
            Actor.OnSkillBeginAttack();
            _isPlaySkill = true;
        }

        protected override IEnumerator IEPerform()
        {
            if (!string.IsNullOrEmpty(animation))
            {
                Actor.PlayAnimation(animation, 0, 1, false, false);
            }

            var posToTeleport = Vector3.zero;
            TryPlayFX(fxPreTeleport, Actor.Position);
            if(prepareDestinationOnStart)
            {
                posToTeleport = GetTeleportPosition();
                TryPlayFX(fxDestination, posToTeleport);
            }
            
            yield return new WaitForSeconds(delayTime);
            
            TryPlayFX(fxStartTeleport, Actor.Position);

            Actor.TurnOffVisual();
            
            yield return new WaitForSeconds(delayPrepareDestination);

            if (!prepareDestinationOnStart)
            {
                posToTeleport = GetTeleportPosition();
                TryPlayFX(fxDestination, posToTeleport);
            }
            
            yield return new WaitForSeconds(delayAppear);
            
            if (NavMesh.SamplePosition(posToTeleport, out var realPos, radiusPos, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                Actor.Warp(realPos.position);
            }
            TryPlayFX(fxAppear, posToTeleport);
            
            Actor.TurnOnVisual();
            if (fxStartTeleport)
            {
                var tele = pools.Spawn(fxStartTeleport);
                tele.transform.position = Actor.LockPosition;
                tele.Play();
            }
            
            yield return new WaitForSeconds(delayMoveNextSkill);
            _isPlaySkill = false;
            Actor.OnSkillCanMoveNextSkill();
            yield return null;
        }

        public override void OnFeedbackEvent(int index)
        {
            base.OnFeedbackEvent(index);
            if (index < 0 || index >= feedbacks.Length)
                return;
            var feedback = feedbacks[index];
            feedback?.Play();
        }

        void TryPlayFX(ParticleSystem fxPrefab, Vector3 position)
        {
            if (fxPrefab)
            {
                var fx = pools.Spawn(fxPrefab);
                fx.transform.position = position;
                fx.Play();
            }
        }

        Vector3 GetTeleportPosition()
        {
            if(Actor.AimedEnemy == null) return Actor.Position;
            switch (teleportType)
            {
                case TeleportType.BehindTarget:
                    return Actor.AimedEnemy.Position - Actor.AimedEnemy.Transform.forward * offset;
                case TeleportType.FrontTarget:
                    return Actor.AimedEnemy.Position + Actor.AimedEnemy.Transform.forward * offset;
                case TeleportType.RandomAroundTarget:
                    return Actor.AimedEnemy.Position + SimpleMath.RandomOnCircleXZ() * offset;
                default:
                    return Actor.Position;
            }
        }

        protected override void DoStop()
        {
            base.DoStop();
            OnStop();
        }

        void OnStop()
        {
            Actor.TurnOnHitBox();
            Actor.EnableMovement();
            Actor.OnSkillEndAttack();
        }
    }
    public enum TeleportType
    {
        BehindTarget, FrontTarget, RandomAroundTarget
    }
}