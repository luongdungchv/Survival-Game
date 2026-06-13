using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    public class ExplosionParticle : BaseMono
    {
        [SerializeField] ParticleSystem[] ground;

        public void Setup()
        {
            var pos = transform.position;
            var isOnGround = pos.y <= 0.5f;
            foreach (var g in ground)
            {
                var e = g.emission;
                e.enabled = isOnGround;
                if (isOnGround)
                {
                    var t = g.transform;
                    var gPos = t.position;
                    gPos.y = 0.1f;
                    t.position = gPos;
                }
            }
        }
    }
}