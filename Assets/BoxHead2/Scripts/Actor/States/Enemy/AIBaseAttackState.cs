using Dacodelaac.FiniteStateMachine;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class AIBaseAttackState : AIBaseState
    {
        int _nextComboIndex;

        public AIBaseAttackState(AIEnemy actor) : base(actor)
        {
        }
        
        protected override void OnStateEnter(BaseState from, object data)
        {
            base.OnStateEnter(from, data);
            AIEnemy.StopMovement();
            AIEnemy.PrepareCurrentComboSkill(false);
            AIEnemy.UseSkill(AIEnemy.CurrentSkillCombo.Skills[0]);
            AIEnemy.UpdateDefaultPosition();
            _nextComboIndex = 1;
        }

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (AIEnemy.IsCanMoveNextSkill && _nextComboIndex < AIEnemy.CurrentSkillCombo.Skills.Length)
            {
                AIEnemy.UseSkill(AIEnemy.CurrentSkillCombo.Skills[_nextComboIndex]);
                _nextComboIndex++;
            }

            if (AIEnemy.CurrentSkill == null || AIEnemy.CurrentSkill.IsFinalStopConditionMet)
            {
                AIEnemy.UpdateLocomotion(Vector2.zero);
                if (AIEnemy.Animator.GetCurrentAnimatorStateInfo(0).IsName("locomotion"))
                {
                    ChangeState<IIdleState>();   
                }
            }
        }

        protected override void OnStateExit(BaseState to)
        {
            base.OnStateExit(to);
            AIEnemy.OnExitSkillState();
            AIEnemy.CurrentSkillCombo.ResetCooldown();
            AIEnemy.CurrentSkillCombo = null;
            AIEnemy.StopCurrentSkill();
            if (AIEnemy.Weapon != null)
            {
                AIEnemy.Weapon.ResetSkillCombo();
            }
        }
    }
}