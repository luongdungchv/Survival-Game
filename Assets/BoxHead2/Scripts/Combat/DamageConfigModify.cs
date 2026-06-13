using System;
using UnityEngine;

namespace BoxHead2.Combat
{
    [Serializable]
    public class DamageConfigModify
    {
        [SerializeField] DamageConfig modify;
        
        public DamageConfig Apply(DamageConfig damageConfig)
        {
            damageConfig.damage += modify.damage;
            damageConfig.criticalChance += modify.criticalChance;
            damageConfig.staggerChance += modify.staggerChance;
            damageConfig.slowChance += modify.slowChance;
            
            return damageConfig;
        }
    }
}