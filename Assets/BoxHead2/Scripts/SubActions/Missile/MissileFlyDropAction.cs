using BoxHead2.Actor;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileFlyDropAction")]
    public class MissileFlyDropAction : MissileFlyStraightAction
    {
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
            if (SimpleMath.InRange(Missile.Transform.position, Destination, 0.1f, true))
            {
                Missile.Despawn();
            }
        }
    }
}