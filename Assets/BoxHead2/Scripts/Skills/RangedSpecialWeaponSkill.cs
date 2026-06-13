namespace BoxHead2.Skills
{
    public class RangedSpecialWeaponSkill : RangedSkill
    {
        private bool _shoot;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _shoot = false;
        }

        protected override MissileData GetMissileData(int index)
        {
            return base.GetMissileData(SpecialHitData);
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            _shoot = true;
        }

        public override void OnRollCancel()
        {
            if (!_shoot)
            {
                OnShoot(0);
            }
        }
    }
}