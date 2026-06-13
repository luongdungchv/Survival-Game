using BoxHead2.Actor;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public class MissileFlyStraightSlowOnHitAction : MissileFlyStraightAction
    {
        [SerializeField] float speedOnHit;
        [SerializeField] float durationAfterHit;
        protected override float Speed => _isHit ? speedOnHit : base.Speed;
        bool _isHit;
        float _timeHit;
        public override void Prepare(object target)
        {
            base.Prepare(target);
            _isHit = false;
        }

        public override void OnHit(object target)
        {
            base.OnHit(target);
            _isHit = true;
            _timeHit = Time.time;
        }

        public override void Trigger(object target, IActor actor)
        {
            var pos = Missile.Transform.position;
            pos = Vector3.MoveTowards(pos, Destination, Speed * Time.deltaTime);
            if (pos.y > limitHeight)
            {
                pos.y = Mathf.Lerp(pos.y, limitHeight, 10 * Time.deltaTime);
            }
            Missile.Transform.position = pos;
            Missile.CurrentDirection = Missile.Transform.forward;
            if (!_isHit && SimpleMath.InRange(Missile.Transform.position, Destination, 0.1f))
            {
                Missile.Despawn();
            }
            else if (_isHit && Time.time - _timeHit >= durationAfterHit)
            {
                Missile.Despawn();
            }
        }
    }
}