using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/MultiSubAction")]
    public class MultiSubAction : SubAction
    {
        [SerializeField] public SubAction[] subActions;
        
        public override T CreateCopy<T>()
        {
            var clone = base.CreateCopy<MultiSubAction>();
            clone.subActions = new SubAction[subActions.Length];
            for (var i = 0; i < clone.subActions.Length; i++)
            {
                clone.subActions[i] = subActions[i].CreateCopy<SubAction>();
            }

            return clone as T;
        }

        public override void Prepare(object target)
        {
            base.Prepare(target);
            foreach (var subAction in subActions)
            {
                subAction.Prepare(target);
            }
        }

        public override void Trigger(object target, IActor actor)
        {
            if (!CanTrigger(actor)) return;
            foreach (var subAction in subActions)
            {
                subAction.Trigger(target, actor);
            }
        }

        public override void Detach(object target)
        {
            foreach (var subAction in subActions)
            {
                subAction.Detach(target);
            }
        }

        public override void Attach(object target)
        {
            foreach (var subAction in subActions)
            {
                subAction.Attach(target);
            }
        }
    }
}