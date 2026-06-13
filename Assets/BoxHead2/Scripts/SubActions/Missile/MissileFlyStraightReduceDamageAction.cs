using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileFlyStraightReduceDamageAction")]
    public class MissileFlyStraightReduceDamageAction : MissileFlyStraightAction
    {
        [SerializeField] float damageReducePerHit;
        [SerializeField] int maxHitCount;

        int _hitCount;

        public override void Prepare(object target)
        {
            base.Prepare(target);
            _hitCount = 0;
        }

        public override void OnHit(object target)
        {
            base.OnHit(target);

            if (_hitCount < maxHitCount)
            {
                _hitCount++;
                var sourceData = Missile.SourceData;
                sourceData.FinalDamageBonus += damageReducePerHit;
                Missile.SourceData = sourceData;
            }
        }
    }
}