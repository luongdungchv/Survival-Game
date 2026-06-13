using BoxHead2.Actor;
using BoxHead2.Combat;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/AreaOfEffectAction")]
    public class AreaOfEffectAction : SubAction
    {
        [SerializeField] protected LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] AreaOfEffectData areaOfEffectData;
        [SerializeField] AreaOfEffect areaOfEffectPrefab;
        [SerializeField] SubAction endAction;
        [SerializeField] bool followTarget;
        [SerializeField] LayerMask ground;
        [SerializeField] bool isTriggerOnce;
        [SerializeField] bool notCheckGround;
        [SerializeField] bool useCustomRotation;
        
        bool _isTriggered;
        public override T CreateCopy<T>()
        {
            var copy = base.CreateCopy<AreaOfEffectAction>();
            if (endAction)
            {
                copy.endAction = endAction.CreateCopy<SubAction>();
            }
            return copy as T;
        }

        public override void Prepare(object target)
        {
            base.Prepare(target);
            _isTriggered = false;
        }

        public override void Trigger(object target, IActor actor)
        {
            if (isTriggerOnce && _isTriggered) return;
            _isTriggered = true;
            
            var data = Get<AreaOfEffectActionData>(target);
            var damageDealtData = data.SourceData;
            var pos = data.Position;
            var radius = areaOfEffectData.Radius * (1 + data.RadiusBonus);
            if (!notCheckGround)
            {
                if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down * 10f, out var hit, 10, ground))
                {
                    pos.y = hit.point.y + 0.1f;
                    var areaOfEffect = pools.Spawn(areaOfEffectPrefab);
                    var aoeData = areaOfEffectData;
                    aoeData.OverrideRadius(radius);
                    
                    areaOfEffect.Transform.position = pos;
                    if(useCustomRotation)
                        areaOfEffect.Transform.eulerAngles =
                            areaOfEffect.Transform.eulerAngles.Set(y: data.HorizontalRotation);
                    areaOfEffect.Setup(followTarget ? data.FollowTarget : null, aoeData, damageDealtData,
                        actor.TeamConfig.GetLayerMask(damageLayer), endAction);
                    if (displayText.ShouldDisplay)
                    {
                        displayText.Display(actor);
                    }
                }
            }
            else
            {
                var areaOfEffect = pools.Spawn(areaOfEffectPrefab);
                var aoeData = areaOfEffectData;
                aoeData.OverrideRadius(radius);
                
                areaOfEffect.Transform.position = pos;
                if(useCustomRotation)
                    areaOfEffect.Transform.eulerAngles =
                        areaOfEffect.Transform.eulerAngles.Set(y: data.HorizontalRotation);
                
                areaOfEffect.Setup(followTarget ? data.FollowTarget : null, aoeData, damageDealtData,
                    actor.TeamConfig.GetLayerMask(damageLayer), endAction);
                if (displayText.ShouldDisplay)
                {
                    displayText.Display(actor);
                }
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void IncreaseDamage()
        {
            var damageData = areaOfEffectData.damageData;
            for (var i = 0; i < damageData.Length; i++)
            {
                damageData[i].damage++;
            }
            areaOfEffectData.damageData = damageData;
            EditorUtility.SetDirty(this);
        }
        
        [Sirenix.OdinInspector.Button]
        public void ReduceDamage()
        {
            var damageData = areaOfEffectData.damageData;
            for (var i = 0; i < damageData.Length; i++)
            {
                damageData[i].damage--;
            }
            areaOfEffectData.damageData = damageData;
            EditorUtility.SetDirty(this);
        }
#endif
    }
    
    public struct AreaOfEffectActionData
    {
        public Transform FollowTarget { get; }
        public Vector3 Position { get; }
        public DamageSourceData SourceData { get; }
        public float RadiusBonus { get; }
        public float HorizontalRotation { get; }

        public AreaOfEffectActionData(Transform followTarget, Vector3 position, DamageSourceData sourceData, float radiusBonus, float horizontalRotation = 0)
        {
            FollowTarget = followTarget;
            Position = position;
            SourceData = sourceData;
            RadiusBonus = radiusBonus;
            HorizontalRotation = horizontalRotation;
        }
    }
}