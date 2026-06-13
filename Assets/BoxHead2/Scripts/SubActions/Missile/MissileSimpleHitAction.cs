using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileSimpleHitAction")]
    public class MissileSimpleHitAction : SubAction
    {
        [Header("Damage")] 
        [SerializeField] Feedback hitFeedback;
        [SerializeField] ParticleSystem hitFxPrefab;
        [SerializeField] LayerMaskType damageLayer = LayerMaskType.DamageLayer;
        [SerializeField] bool despawnOnHit = true;
        
        Missile missile;

        public override void Trigger(object target, IActor actor)
        {
            var data = Get<MissileHitActionData>(target);
            
            if (hitFxPrefab)
            {
                FxHelper.SpawnFx(pools, data.HitPosition, missile.Transform.forward, hitFxPrefab, null);
            }

            if (hitFeedback)
            {
                hitFeedback.Play();
            }
            
            if (despawnOnHit)
            {
                missile.Despawn();
            }
        }

        public override void Prepare(object target)
        {
            var data = Get<MissileSetupActionData>(target);
            missile = data.Missile;
            missile.gameObject.layer = data.SetupData.Actor.TeamConfig.GetLayerMask(damageLayer).ToGameObjectLayer();
        }
    }
}