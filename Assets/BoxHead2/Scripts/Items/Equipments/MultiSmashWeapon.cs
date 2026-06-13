using UnityEngine;

namespace BoxHead2.Items
{
    public class MultiSmashWeapon : RangedWeapon
    {
        [SerializeField] Transform[] localHitPoints;

        public Transform[] LocalHitPoints => localHitPoints;
    }
}