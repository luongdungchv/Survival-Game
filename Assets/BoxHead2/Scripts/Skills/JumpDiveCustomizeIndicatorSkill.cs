using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Skills
{
    public class JumpDiveCustomizeIndicatorSkill : JumpDiveSkill
    {
        [SerializeField] float moveTimeMultiplier = 1;
        bool _canUpdateIndicator;
        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _canUpdateIndicator = false;
            moveTime *= moveTimeMultiplier;
        }

        protected override void UpdateIndicator()
        {
            if (!_canUpdateIndicator) return;
            if (NavMesh.SamplePosition(targetPos, out var hit, 30f, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                targetPos = hit.position;
            }

            targetDirection.y = 0;
            base.UpdateIndicator();
        }
        

        public override void OnBeginHit(int index)
        {
            base.OnBeginHit(index);
            _canUpdateIndicator = true; 
        }
    }
}