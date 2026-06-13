using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Combat;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/DelayAction")]
    public class DelayAction : SubAction, ISubActionDataProvider<ExplodeActionData>, ISubActionDataProvider<AreaOfEffectActionData>
    {
        [SerializeField] float delay;
        [SerializeField] SubAction subAction;
        [Header("Feedback")]
        [SerializeField] GameObject fxPrefab;

        DelayActionData data;
        IActor actor;

        public override T CreateCopy<T>()
        {
            var copy = base.CreateCopy<DelayAction>();
            copy.subAction = subAction.CreateCopy<SubAction>();
            return copy as T;
        }

        public override void Trigger(object target, IActor actor)
        {
            if (!CanTrigger(actor)) return;

            data = Get<DelayActionData>(target);
            this.actor = actor;
            if (fxPrefab)
            {
                var fx = pools.Spawn(fxPrefab);
                fx.transform.position = data.Position;
            }
            StartPerformRoutine();
            if (displayText.ShouldDisplay)
            {
                displayText.Display(actor);
            }
        }

        public override void Detach(object target)
        {
            StopPerformRoutine();
        }

        public override void Attach(object target)
        {
            StopPerformRoutine();
        }

        protected override IEnumerator IEPerform()
        {
            yield return new WaitForSeconds(delay);
            subAction.Trigger(this, actor);
        }

        ExplodeActionData ISubActionDataProvider<ExplodeActionData>.Get()
        {
            return new ExplodeActionData(data.Position, data.Position, 0, data.SourceData, new DamageConfig(), false);
        }
        
        AreaOfEffectActionData ISubActionDataProvider<AreaOfEffectActionData>.Get()
        {
            return new AreaOfEffectActionData(null, data.Position, data.SourceData, 0);
        }
    }
    
    public class DelayActionData
    {
        public Vector3 Position;
        public IActor Actor;
        public DamageSourceData SourceData;

        public DelayActionData(Vector3 position, IActor actor, DamageSourceData sourceData)
        {
            Position = position;
            Actor = actor;
            SourceData = sourceData;
        }
    }
}