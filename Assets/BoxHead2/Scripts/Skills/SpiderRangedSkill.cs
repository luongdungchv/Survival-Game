using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/SpiderRanged")]

    public class SpiderRangedSkill : RangedSkill
    {
        public override bool IsStopConditionMet => _isStop;
        bool _isStop;
        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _isStop = false;
        }

        protected override void AfterDespawnMissileAction(MissileHitData missileHitData)
        {
            base.AfterDespawnMissileAction(missileHitData);
            _isStop = true;
            if (!Actor.Alive) return;
            Actor.OnSkillEndAttack();
            if (Actor.AimedEnemy is { IsStaggering: true })
            {
                Actor.OnSkillCanMoveNextSkill();
            }
        }
    }
}