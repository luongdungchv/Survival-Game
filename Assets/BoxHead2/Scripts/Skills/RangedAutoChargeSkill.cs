using System.Collections;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class RangedAutoChargeSkill : RangedSkill
    {
        [SerializeField] protected string releaseAnim;
        [SerializeField] protected float holdDuration;
        [SerializeField] protected GameObject shootIndicatorPrefab;
        [SerializeField] Feedback chargeFeedback;
        [SerializeField] protected float releaseAnimSpeed = 1;
        
        protected bool _charging;
        protected float _value = 1f;
        
        protected Coroutine _holdCoroutine;
        
        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _charging = false;
            _value = 0;
            tracking = true;
        }

        protected override void DoStop()
        {
            base.DoStop();
            if (_holdCoroutine != null)
                Actor.StopCoroutine(_holdCoroutine);
        }

        protected override IEnumerator IEPerform()
        {
            knockBack = 0;
            knockBackDir = Vector3.zero;
            Actor.PlayAnimation(animation, 0, AnimSpeed, animFade, false);
            Actor.SetRootMotionMult(rootMotionMult);

            while (true)
            {
                if (tracking)
                {
                    aimPos = GetAimedPosition();
                    var startPos = Actor.Position.Set(y: weapons[0].LauncherPosition.y);
                    aimDir = aimPos - startPos;
                    Actor.RotateDirection(aimDir, trackingSpeed);
                    if (showAimLine && weapons != null)
                    {
                        foreach (var weapon in weapons)
                        {
                            weapon.ShowIndicator(aimDir, aimPos, Range, spreadCount, spreadAngleStep);
                        }
                    }
                }

                if (_charging)
                {
                    UpdateIndicator(aimDir);
                }
                if (knockBack > 0)
                {
                    Actor.MoveDirection(knockBackDir, knockBack);
                    knockBack -= knockBack * 10 * Time.deltaTime;
                }
                yield return null;
            }
        }
        
        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            weapons[0].StartCharge();
            _charging = true;
            _holdCoroutine = Actor.StartCoroutine(IEHoldBeforeShoot());
            chargeFeedback?.Play();
        }
        
        IEnumerator IEHoldBeforeShoot()
        {
            yield return new WaitForSeconds(holdDuration);
            if(!string.IsNullOrEmpty(releaseAnim)) 
                Actor.PlayAnimation(releaseAnim, 0, releaseAnimSpeed, animFade, false);
        }
    }
}