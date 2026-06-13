using UnityEngine;

namespace BoxHead2.Skills
{
    public class KrakenRangedSkill : RangedSkill
    {
        public static int KrakenRangedSkillCount { get; set; }
        public static int MaxUseCount = 2;
        public static bool CanUseSkill = KrakenRangedSkillCount < MaxUseCount;
        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            KrakenRangedSkillCount++;
        }

        protected override void DoStop()
        {
            base.DoStop();
            KrakenRangedSkillCount--;
        }
        
    }
}