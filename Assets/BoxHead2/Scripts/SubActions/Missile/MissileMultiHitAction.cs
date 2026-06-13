using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public class MissileMultiHitAction : MissileHitAction
    {
        [SerializeField] float hitInterval;

        float _lastTimeHit;
        public override void Trigger(object target, IActor actor)
        {
            _damageTakerSet?.Clear();
            var delta = Time.time - _lastTimeHit;
            if (delta < hitInterval) return;
            _lastTimeHit = Time.time;
            base.Trigger(target, actor);
        }
    }
}