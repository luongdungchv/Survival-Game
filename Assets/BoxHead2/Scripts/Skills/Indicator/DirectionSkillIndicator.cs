using UnityEngine;

namespace BoxHead2.Skills
{
    public class DirectionSkillIndicator : SkillIndicator
    {
        [Header("Main")] 
        [SerializeField] float directionLengthScale;
        [SerializeField] float directionWidthScale;
        [SerializeField] bool scaleWidth;
        [SerializeField] bool isSwap;
        [SerializeField] SpriteRenderer directionIndicator;
        [SerializeField] bool snapToGround;
        [SerializeField] LayerMask ground;
        [Header("Left-Right")] 
        [SerializeField] Transform left;
        [SerializeField] Transform right;
        [SerializeField] float leftRightLengthOffset;

        public override void Set(Vector3 pos, Vector3 launchPos, Vector3 targetPos, Vector3 dir, float range,
            float radius, float originRadius, float spreadAngle, float spreadAngleOrigin)
        {
            pos.y += 0.1f;
            if (snapToGround &&
                Physics.Raycast(pos + Vector3.up, Vector3.down * 10f, out var hit, 10, ground))
            {
                pos.y = hit.point.y + 0.1f;
            }
            
            Transform.position = pos;
            Transform.rotation = Quaternion.LookRotation(dir);
            if (directionIndicator)
            {
                directionIndicator.size = isSwap
                    ? new Vector2(scaleWidth ? originRadius * directionWidthScale : directionWidthScale,
                        range * directionLengthScale)
                    : new Vector2(range * directionLengthScale,
                        scaleWidth ? originRadius * directionWidthScale : directionWidthScale);
            }

            if (left && right)
            {
                left.localScale = right.localScale = new Vector3(1, 1, range - leftRightLengthOffset);
                var offsetX = Mathf.Lerp(0, originRadius / 2, radius / originRadius);
                left.localPosition = new Vector3(-offsetX, 0, 0);
                right.localPosition = new Vector3(offsetX, 0, 0);
            }
        }
    }
}