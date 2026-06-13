using BoxHead2.Helper;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    public class ColliderHitFx : BaseMono
    {
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] Feedback hitFeedback;

        public void OnHit(Vector3 pos, Vector3 dir)
        {
            FxHelper.SpawnFx(pools, pos, dir, hitFxPrefab, hitFeedback);
        }
    }
}