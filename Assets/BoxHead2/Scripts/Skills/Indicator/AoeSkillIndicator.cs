using UnityEngine;

namespace BoxHead2.Skills
{
    public class AoeSkillIndicator : SkillIndicator
    {
        [SerializeField] Transform aoeIndicator;
        [SerializeField] LineRenderer line;
        [SerializeField] float angle;
        [SerializeField] float gravity;
        [SerializeField] bool snapToGround;
        [SerializeField] LayerMask ground;

        public override void Set(Vector3 pos, Vector3 launchPos, Vector3 targetPos, Vector3 dir, float range, float radius, float originRadius, float spreadAngle, float spreadAngleOrigin)
        {
            pos.y += 0.1f;
            if (snapToGround &&
                Physics.Raycast(targetPos + Vector3.up, Vector3.down * 10f, out var hit, 10, ground))
            {
                targetPos.y = hit.point.y + 0.1f;
            }
            
            Transform.position = pos;
            aoeIndicator.position = targetPos;
            aoeIndicator.localScale = Vector3.one * radius;

            if (line != null)
            {

                line.positionCount = 10;

                var r = targetPos - launchPos;
                r.y = 0;
                var l = r.magnitude;
                if (l > range)
                {
                    l = range;
                }

                var a = angle * Mathf.Deg2Rad;
                var h = -launchPos.y;
                var v = Mathf.Sqrt(gravity * l * l /
                                   (2 * (Mathf.Sin(a) / Mathf.Cos(a) * l - h) * Mathf.Cos(a) * Mathf.Cos(a)));
                r.y = l * Mathf.Tan(a);
                r.Normalize();
                var velocity = r * v;
                var vx = new Vector3(velocity.x, 0, velocity.z);
                var vy = velocity.y;
                var deltaT = l / v / line.positionCount;

                line.SetPosition(0, launchPos);
                line.SetPosition(line.positionCount - 1, targetPos);

                for (var i = 1; i < line.positionCount - 1; i++)
                {
                    var time = i * deltaT;
                    var currentPos = launchPos + vx * time + (vy * time - 0.5f * gravity * time * time) * Vector3.up;
                    line.SetPosition(i, currentPos);
                }
            }
        }
    }
}