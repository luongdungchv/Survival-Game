using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class MeleeSkillLeaveAOE : MeleeSkill, ISubActionDataProvider<AreaOfEffectActionData>
    {
        [Header("Specific Properties")] 
        [SerializeField] AreaOfEffectAction aoeAction;

        AreaOfEffectAction _activeAoeAction;
        Vector3 _cachedIndicatorPos;
        float _cachedIndicatorEulerY;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _activeAoeAction ??= aoeAction.CreateCopy<AreaOfEffectAction>();
            _activeAoeAction.Prepare(this);
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if (_activeAoeAction)
            {
                _activeAoeAction.Trigger(this, Actor);
            }            
        }

        public override void OnStopIndicator(int index)
        {
            _cachedIndicatorPos = _attackIndicator.transform.position;
            _cachedIndicatorEulerY = _attackIndicator.transform.eulerAngles.y;
            base.OnStopIndicator(index);
        }

        AreaOfEffectActionData ISubActionDataProvider<AreaOfEffectActionData>.Get()
        {
            var sourceData = (Actor as IDamageSource).GetDamageSourceData();
            return new AreaOfEffectActionData(null, _cachedIndicatorPos, sourceData, 0, _cachedIndicatorEulerY);
        }
    }
}