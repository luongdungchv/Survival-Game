using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Dacodelaac.Utils
{
    public static class DOTweenHelper
    {
        public static IEnumerator WaitForTween(List<Tween> tweens)
        {
            yield return new WaitUntil(() => tweens.All(t => t == null || !t.IsActive() || t.IsComplete()));
        }
        
        public static IEnumerator WaitForTween(Tween tween)
        {
            yield return new WaitUntil(() => tween == null || !tween.IsActive() || tween.IsComplete());
        }

        public static IEnumerator WaitForCompleted(this Tween tween)
        {
            yield return WaitForTween(tween);
        }
    }
}