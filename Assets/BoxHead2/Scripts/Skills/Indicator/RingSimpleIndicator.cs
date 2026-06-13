using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class RingSimpleIndicator : BaseMono
    {
        [SerializeField] GameObject indicator;
        [SerializeField] GameObject outsideIndicator;
        
        float _radius;
        public void Setup(float radius)
        {
            _radius = radius;
            outsideIndicator.transform.localScale = _radius * Vector3.one;
            indicator.transform.localScale = Vector3.zero;
        }

        public void UpdateProgress(float progress)
        {
            indicator.transform.localScale = progress * _radius * Vector3.one;
        }
    }
}