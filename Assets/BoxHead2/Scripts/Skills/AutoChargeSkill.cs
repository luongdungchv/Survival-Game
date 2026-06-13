using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class AutoChargeSkill : Skill
    {
        [SerializeField] string animation;
        [SerializeField] protected string releaseAnim;
        [SerializeField] protected string loopAnim;
        [SerializeField] protected float holdDuration;
        [SerializeField] protected GameObject shootIndicatorPrefab;
        [SerializeField] Feedback chargeFeedback;
        [SerializeField] Feedback shootFeedback;
        
        protected bool _charging;
        protected bool tracking;
        protected float _value = 1f;

        protected Vector3 aimPos, aimDir;
        
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        
        protected override IEnumerator IEPerform()
        {
            Actor.PlayAnimation(animation, 0, 1, false, false);
            Actor.SetRootMotionMult(rootMotionMult);

            while (true)
            {
                if (tracking)
                {
                    aimPos = GetAimedPosition();
                    var startPos = Actor.Position;
                    aimDir = aimPos - startPos;
                    Actor.RotateDirection(aimDir, 999);
                }

                if (_charging)
                {
                    UpdateIndicator(aimDir);
                }

                OnPerformUpdate();
                yield return null;
            }
        }
        
        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            _charging = true;
            Actor.PlayAnimation(loopAnim, index, 1, false, false);
            Actor.StartCoroutine(IEHoldBeforeShoot());
            chargeFeedback?.Play();
        }
        
        protected IEnumerator IEHoldBeforeShoot()
        {
            yield return new WaitForSeconds(holdDuration);
            if(!string.IsNullOrEmpty(releaseAnim)) 
                Actor.PlayAnimation(releaseAnim, 0, 1, false, false);
        }

        protected void PlayShootFeedback()
        {
            shootFeedback?.Play();
        }

        protected virtual void UpdateIndicator(Vector3 aimDir)
        {
            
        }

        protected virtual void OnPerformUpdate()
        {
            
        }
    }
}