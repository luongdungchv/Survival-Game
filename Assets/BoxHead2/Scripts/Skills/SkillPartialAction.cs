using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class SkillPartialAction : SubAction
    {
        [SerializeField] int index = 999;

        public int Index => index;

        protected Skill Skill;
        protected IActor Actor;

        public override void Trigger(object target, IActor actor)
        {
            Skill = Get<Skill>(target);
            Actor = Skill.Actor;
            if (displayText.ShouldDisplay)
            {
                displayText.Display(actor);
            }
        }

        protected DamageSourceData GetDamageSourceData()
        {
            return (Actor as IDamageSource).GetDamageSourceData();
        }
    }
}