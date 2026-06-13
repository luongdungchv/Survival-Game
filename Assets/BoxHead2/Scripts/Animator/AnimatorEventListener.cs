using System;
using Dacodelaac.Core;

namespace BoxHead2.AnimatorEventCustom
{
    public class AnimatorEventListener : BaseMono
    {
        public event Action<int> OnBeginHitEvent;
        public event Action<int> OnStopIndicatorEvent;
        public event Action<int> OnBeginTrailEvent;
        public event Action<int> OnStopTrailEvent; 
        public event Action<int> OnBeginMoveEvent;
        public event Action<int> OnStopMoveEvent;
        public event Action<int> OnBeginTrackingEvent;
        public event Action<int> OnStopTrackingEvent;
        public event Action OnBeginAttackEvent;
        public event Action OnEndAttackEvent;
        public event Action OnCanMoveNextSkillEvent;
        public event Action OnEnterLocomotionStateEvent;
        public event Action OnAnimatorMoveEvent;
        public event Action<int> OnPrepareShootEvent;
        public event Action<int> OnChargeFullEvent;
        public event Action<int> OnShootEvent;
        public event Action<int> OnCustomEventEvent;
        public event Action<int> OnFeedbackEvent;
        public event Action<int> OnFootEvent;
        public event Action OnJumpLandingEvent;
        public event Action OnShowWarningEvent;


        public void OnBeginHit(int index)
        {
            OnBeginHitEvent?.Invoke(index);
        }

        public void OnShowWarning()
        {
            OnShowWarningEvent?.Invoke();
        }
        
        public void OnStopIndicator(int index)
        {
            OnStopIndicatorEvent?.Invoke(index);
        }
        
        public void OnBeginMove(int index)
        {
            OnBeginMoveEvent?.Invoke(index);
        }
        
        public void OnStopMove(int index)
        {
            OnStopMoveEvent?.Invoke(index);
        }
        
        public void OnBeginTracking(int index)
        {
            OnBeginTrackingEvent?.Invoke(index);
        }
        
        public void OnStopTracking(int index)
        {
            OnStopTrackingEvent?.Invoke(index);
        }
        
        public void OnBeginTrail(int index)
        {
            OnBeginTrailEvent?.Invoke(index);
        }

        public void OnStopTrail(int index)
        {
            OnStopTrailEvent?.Invoke(index);
        }

        public void OnBeginAttack()
        {
            OnBeginAttackEvent?.Invoke();
        }

        public void OnEndAttack()
        {
            OnEndAttackEvent?.Invoke();
        }

        public void OnCanMoveNextSkill()
        {
            OnCanMoveNextSkillEvent?.Invoke();
        }

        public void OnPrepareShoot(int index)
        {
            OnPrepareShootEvent?.Invoke(index);
        }
        
        public void OnChargeFull(int index)
        {
            OnChargeFullEvent?.Invoke(index);
        }

        public void OnShoot(int index)
        {
            OnShootEvent?.Invoke(index);
        }

        public void OnCustomEvent(int index)
        {
            OnCustomEventEvent?.Invoke(index);
        }

        public void OnFeedback(int index)
        {
            OnFeedbackEvent?.Invoke(index);
        }

        public void OnFoot(int index)
        {
            OnFootEvent?.Invoke(index);
        }
        
        public void OnJumpLanding()
        {
            OnJumpLandingEvent?.Invoke();
        }

        public void OnEnterLocomotionState()
        {
            OnEnterLocomotionStateEvent?.Invoke();
        }

        protected void TriggerAnimatorMoveEvent()
        {
            OnAnimatorMoveEvent?.Invoke();
        }
    }
}