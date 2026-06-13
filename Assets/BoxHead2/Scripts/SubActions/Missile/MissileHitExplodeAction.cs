using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MissileHitExplodeAction")]
    public class MissileHitExplodeAction : ExplodeAction
    {
        [SerializeField] LayerMaskType damageMissileLayer = LayerMaskType.DamageLayer;
        [SerializeField] bool despawnOnHit = true;
        
        HashSet<IDamageTaker> _damageTakerSet;
        Missile _missile;
        public override void Prepare(object target)
        {
            base.Prepare(target);
            var data = Get<MissileSetupActionData>(target);
            _damageTakerSet = new HashSet<IDamageTaker>();
            _missile = data.Missile;
            _missile.gameObject.layer = data.SetupData.Actor.TeamConfig.GetLayerMask(damageMissileLayer).ToGameObjectLayer();
        }

        public override void Trigger(object target, IActor actor)
        {
            var data = Get<MissileHitActionData>(target);
            if (data.DamageTaker == null)
            {
                if (despawnOnHit)
                {
                    _missile.Despawn();
                }
                return;
            }
            
            if (_damageTakerSet.Contains(data.DamageTaker)) return;
            
            _damageTakerSet.Add(data.DamageTaker);
            
            base.Trigger(target, actor);
        }
    }
}