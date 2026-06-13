using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class MaterialModifyAttackIndicator : AttackIndicator
    {
        [SerializeField] float duration;
        [SerializeField] Renderer indicatorRenderer;
        [SerializeField] string valueName;
        [SerializeField] string colorName;
        [SerializeField] bool usePresetValues;
        [SerializeField] bool updateRadius;
        [SerializeField, ShowIf("usePresetValues")] Color startColor, endColor;
        [SerializeField, ShowIf("usePresetValues")] float startValue, endValue;
        
        AttackIndicatorData data;
        float _startTime = 0;
        public override void Setup(AttackIndicatorData data)
        {
            this.data = data;
            gameObject.SetActive(false);
            _startTime = 0;
        }
        
        public override void DoUpdate(Transform source, Vector3 targetPos, Vector3 targetDirection, float range, float radius)
        {
            gameObject.SetActive(true);
            _startTime += Time.deltaTime;
            
            SetColor(Color.Lerp(startColor, endColor, _startTime / duration));
            SetFadeValue(Mathf.Lerp(startValue, endValue, _startTime / duration));

            if (updateRadius)
            {
                switch (data.updateRadius)
                {
                    case AttackIndicatorData.UpdateRadius.Fixed:
                        transform.localScale = Vector3.one * data.radius;
                        break;
                    case AttackIndicatorData.UpdateRadius.Runtime:
                        transform.localScale = Vector3.one * radius;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            switch (data.updatePosition)
            {
                case AttackIndicatorData.UpdatePosition.FollowSource:
                    targetPos = source.TransformPoint(data.positionOffset);
                    transform.position = targetPos;
                    break;
                case AttackIndicatorData.UpdatePosition.FollowTarget:
                    targetPos += source ? source.TransformDirection(data.positionOffset) : data.positionOffset;
                    transform.position = targetPos;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Transform.rotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(data.rotationOffset);
        }
        
        void SetColor(Color color)
        {
            var mbp = new MaterialPropertyBlock();
            indicatorRenderer.GetPropertyBlock(mbp);
            mbp.SetColor(colorName, color);
            indicatorRenderer.SetPropertyBlock(mbp);
        }
        
        void SetFadeValue(float value)
        {
            var mbp = new MaterialPropertyBlock();
            indicatorRenderer.GetPropertyBlock(mbp);
            mbp.SetFloat(valueName, value);
            indicatorRenderer.SetPropertyBlock(mbp);
        }
    }
}