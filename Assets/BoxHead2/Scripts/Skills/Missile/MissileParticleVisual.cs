using Dacodelaac.Core;
using DG.Tweening;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class MissileParticleVisual : BaseMono
    {
        public void SetupPath(Vector3 endPos, float duration)
        {
            DOTween.Kill(this);
            transform.DOMove(endPos, duration).SetEase(Ease.Linear).Play().SetTarget(this).OnComplete(() =>
            {
                pools.Despawn(gameObject);
            });
        }
    }
}