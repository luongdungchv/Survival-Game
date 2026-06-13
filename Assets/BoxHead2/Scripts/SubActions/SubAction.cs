using System.Collections;
using BoxHead2.Actor;
using BoxHead2.SubCombat;
using Dacodelaac.Core;
using Dacodelaac.Events;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public abstract class SubAction : BaseSO
    {
        [SerializeField] protected DisplayText displayText;
        [Header("TriggerEvent")]
        [SerializeField] Object objectEventData;
        [SerializeField] ObjectEvent triggerEvent;
        [SerializeField] ObjectEvent stopEvent;
        
        public bool CanTrigger(IActor actor) => true;
        protected Coroutine PerformRoutine;

        public virtual void Prepare(object target)
        {
        }

        public abstract void Trigger(object target, IActor actor);

        public virtual void Detach(object target)
        {
        }

        public virtual void Attach(object target)
        {
        }
        
        protected T Get<T>(object target)
        {
            if (target is ISubActionDataProvider<T> t)
            {
                return t.Get();
            }

            Debug.LogError("Invalid cast sub action target");
            return default;
        }
        
        protected virtual IEnumerator IEPerform()
        {
            yield return null;
        }

        protected void StartPerformRoutine()
        {
            StopPerformRoutine();
            PerformRoutine = ticker.StartCoroutine(IEPerform());
        }

        protected void StopPerformRoutine()
        {
            if (PerformRoutine != null)
            {
                ticker.StopCoroutine(PerformRoutine);
            }
        }

        protected void TriggerEvent()
        {
            if (triggerEvent)
            {
                triggerEvent.Raise(objectEventData);
            }
        }

        protected void StopEvent()
        {
            if (stopEvent)
            {
                stopEvent.Raise(objectEventData);
            }
        }

        public virtual void Swap()
        {
            
        }
    }

    public interface ISubActionDataProvider<out T>
    {
        public T Get();
    }
}