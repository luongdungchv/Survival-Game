using BoxHead2.Items;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Actor
{
    [CreateAssetMenu(menuName = "ActorConfig/AIConfigDetail")]
    public class AIConfigDetail : BaseSO
    {
        [SerializeField] public AIEnemy aiPrefab;
        [SerializeField] public WeaponData weapon;
        [SerializeField] public SpecialSkill[] specialSkills;
        [SerializeField] public SkillCombo[] skillCombos;
    }
}