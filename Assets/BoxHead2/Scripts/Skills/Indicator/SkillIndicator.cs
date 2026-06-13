using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Skills
{
    public abstract class SkillIndicator : BaseMono
    {
        public abstract void Set(Vector3 pos, Vector3 launchPos, Vector3 targetPos, Vector3 dir, float range, float radius, float originRadius, float spreadAngle, float spreadAngleOrigin);
    }
}