using System.Collections;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/BounceBallSkill")]
    public class BounceBallSkill : Skill, ISubActionDataProvider<ExplodeActionData>
    {
        [Header("Anim")]
        [SerializeField] string beginBounceAnimation;
        [SerializeField] string endBounceAnimation;
        [Header("Bounce")] 
        [SerializeField] int maxBounce;
        [SerializeField] float healPercentPerBounce;
        [Header("Hit Action")]
        [SerializeField] SubAction[] hitActions;
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;
        int _bounceCount;
        
        protected override void PrepareSkill()
        {
            Actor.StopMovement();
            _bounceCount = 0;
        }
        
        protected override IEnumerator IEPerform()
        {
            Actor.OnSkillBeginAttack();
            Actor.PlayAnimation(beginBounceAnimation, 0, 1, true, false);
            
            while (_bounceCount < maxBounce)
            {
                yield return null;
            }


            if (!string.IsNullOrEmpty(endBounceAnimation))
            {
                Actor.PlayAnimation(endBounceAnimation, 0, 1, true, false);
            }
            else
            {
                Actor.OnSkillEndAttack();
                Actor.OnSkillCanMoveNextSkill();
            }
        }

        public override void OnCustomEvent(int index)
        {
            base.OnCustomEvent(index);
            //999 is bound landing
            if (index == 999)
            {
                _bounceCount++;
                if (healPercentPerBounce > 0)
                {
                    Actor.Heal(new HealData
                    {
                        Amount = Actor.MaxHp * healPercentPerBounce,
                        ActiveFx = true,
                        HpText = true,
                        Revive = false,
                        ReviveText = false,
                        IsPlaySound = false,
                    });
                }
                foreach (var hitAction in hitActions)
                {
                    hitAction.Trigger(this, Actor);
                }
            }
        }
        
        ExplodeActionData ISubActionDataProvider<ExplodeActionData>.Get()
        {
            return new ExplodeActionData(Actor.Position, Actor.Position, 0, GetDamageSourceData(), new DamageConfig(), false);
        }
    }
}