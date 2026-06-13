﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/Teleport")]
    public class TeleportSkill : Skill
    {
        [SerializeField] float radiusPos;
        [SerializeField] string animation;
        [SerializeField] ParticleSystem teleportEffect;
        [SerializeField] float delayTime = 0.75f;
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
            yield return new WaitForSeconds(delayTime);
            if (teleportEffect)
            {
                var tele = pools.Spawn(teleportEffect);
                tele.transform.position = Actor.LockPosition;
                tele.Play();
            }

            Actor.TurnOffVisual();
            yield return new WaitForSeconds(delayTime);
            var posToTeleport = Actor.AimedEnemy.Position - Actor.AimedEnemy.Transform.forward;
            if (NavMesh.SamplePosition(posToTeleport, out var realPos, radiusPos, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                Actor.Warp(realPos.position);
            }
            
            Actor.TurnOnVisual();
            if (teleportEffect)
            {
                var tele = pools.Spawn(teleportEffect);
                tele.transform.position = Actor.LockPosition;
                tele.Play();
            }
            
            yield return new WaitForSeconds(0.25f);
            _isPlaySkill = false;
            Actor.OnSkillCanMoveNextSkill();
            yield return null;
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
}
