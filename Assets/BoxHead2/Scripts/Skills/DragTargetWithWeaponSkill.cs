using BoxHead2.Helper;
using BoxHead2.Items;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Skills
{
    public abstract class DragTargetWithWeaponSkill : TargetPositionModifySkill
    {
        [SerializeField] AttachConfig[] attachConfigs;
        
        [SerializeField] Feedback[] feedbacks;
        
        protected Weapon weapon;
        protected Vector3 offset;
        
        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            var weapons = SkillHelper.GetAttachedEquipments<Weapon>(Actor, attachConfigs);
            if (weapons != null && weapons.Length > 0)
                weapon = weapons[0];
        }

        public override void OnFeedbackEvent(int index)
        {
            base.OnFeedbackEvent(index);
            if (index < 0 || index >= feedbacks.Length)
                return;
            var feedback = feedbacks[index];
            feedback?.Play();
        }

        protected override void UpdateTargetPosition()
        {
            if (!ModifyTarget || !weapon) return;
            if (NavMesh.SamplePosition(weapon.LauncherPosition + offset, out var hit, 15, NavMesh.AllAreas))
            {
                ModifyTarget.Transform.position = hit.position;
            }
        }
    }
}