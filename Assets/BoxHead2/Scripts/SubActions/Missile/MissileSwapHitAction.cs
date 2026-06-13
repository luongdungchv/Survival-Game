using BoxHead2.Combat;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileSwapHitAction")]
    public class MissileSwapHitAction : MissileHitAction
    {
        [Header("Swap Damage")]
        [SerializeField] DamageConfig swapDamageConfig;
        
        protected override DamageConfig DamageConfig => _isSwap ? swapDamageConfig : base.DamageConfig;
        bool _isSwap;
        public override void Prepare(object target)
        {
            base.Prepare(target);
            _isSwap = false;
        }

        public override void Swap()
        {
            base.Swap();
            _isSwap = true;
        }
    }
}