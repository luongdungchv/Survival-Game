using System;
using BoxHead2.Collection;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Combat
{
    [CreateAssetMenu(menuName = "TeamConfig")]
    public class TeamConfig : BaseSO
    {
        [SerializeField] public DamageTakerCollection allyCollection;
        [SerializeField] public DamageTakerCollection enemyCollection;
        [SerializeField] public DamageTakerCollection breakableCollection;
        [SerializeField] LayerMask damageLayer;
        [SerializeField] LayerMask allyLayer;
        [SerializeField] LayerMask hitBoxLayer;
        [SerializeField] LayerMask enemyHitBoxLayer;
        [SerializeField] int index;
        public int Index => index;

        public LayerMask GetLayerMask(LayerMaskType layerMaskType)
        {
            switch (layerMaskType)
            {
                case LayerMaskType.DamageLayer:
                    return damageLayer;
                case LayerMaskType.HitBoxLayer:
                    return hitBoxLayer;
                case LayerMaskType.EnemyHitBoxLayer:
                    return enemyHitBoxLayer;
                case LayerMaskType.AllyLayer:
                    return allyLayer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layerMaskType), layerMaskType, null);
            }
        }
    }
    
    public enum LayerMaskType
    {
        DamageLayer,
        HitBoxLayer,
        EnemyHitBoxLayer,
        AllyLayer,
    }
}