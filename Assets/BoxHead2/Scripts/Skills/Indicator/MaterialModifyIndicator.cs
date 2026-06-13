using Dacodelaac.Core;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class MaterialModifyIndicator : BaseMono
    {
        [SerializeField] Renderer indicatorRenderer;
        [SerializeField] string valueName;
        [SerializeField] string colorName;
        [SerializeField] bool usePresetValues;

        [SerializeField, ShowIf("usePresetValues")]
        bool autoUpdate;

        [SerializeField, EnableIf("@this.usePresetValues && this.autoUpdate")] float autoUpdateDuration;
        [SerializeField, ShowIf("usePresetValues")] Color startColor, endColor;
        [SerializeField, ShowIf("usePresetValues")] float startValue, endValue;

        public override void Initialize()
        {
            base.Initialize();
            if (autoUpdate) StartUpdate(autoUpdateDuration);
        }

        public void SetFadeValue(float value)
        {
            var mbp = new MaterialPropertyBlock();
            indicatorRenderer.GetPropertyBlock(mbp);
            mbp.SetFloat(valueName, value);
            indicatorRenderer.SetPropertyBlock(mbp);
        }

        public void SetColor(Color color)
        {
            var mbp = new MaterialPropertyBlock();
            indicatorRenderer.GetPropertyBlock(mbp);
            mbp.SetColor(colorName, color);
            indicatorRenderer.SetPropertyBlock(mbp);
        }

        public void UpdateValues(float t)
        {
            var color = Color.Lerp(startColor, endColor, t);
            var val = Mathf.Lerp(startValue, endValue, t);
            SetFadeValue(val);
            SetColor(color);
        }

        public void StartUpdate(float duration)
        {
            if (!gameObject.activeInHierarchy) return;
            SetColor(startColor);
            SetFadeValue(startValue);
            DOTween.To(() => startColor, SetColor, endColor, duration).SetTarget(this).Play();
            DOTween.To(() => startValue, SetFadeValue, endValue, duration).SetTarget(this).Play();
        }
    }
}