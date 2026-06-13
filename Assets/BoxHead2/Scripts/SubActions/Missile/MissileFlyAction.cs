using UnityEngine;
using Event = Dacodelaac.Events.Event;

namespace BoxHead2.SubActions
{
    public abstract class MissileFlyAction : SubAction
    {
        [SerializeField] float flyRange = 50;
        public float FlyRange => flyRange;
        public abstract void OnHit(object target);

        public abstract void OnHitObstacle(object target);

        public abstract void OnMissileDeSpawn(object target);
    }
}