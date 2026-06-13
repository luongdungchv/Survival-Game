namespace BoxHead2.Skills
{
    public class MeleeSpecialWeaponSkill : MeleeSkill
    {
        protected override void SetupCombineDamage(int index)
        {
            base.SetupCombineDamage(SpecialHitData);
        }

        public override void OnBeginTrail(int index)
        {
            if (index == 0)
            {
                base.OnBeginTrail(SpecialHitData);
            }
            else
            {
                base.OnBeginTrail(index);
            }
        }

        public override void OnStopTrail(int index)
        {
            if (index == 0)
            {
                base.OnStopTrail(SpecialHitData);
            }
            else
            {
                base.OnStopTrail(index);
            }
        }
    }
}