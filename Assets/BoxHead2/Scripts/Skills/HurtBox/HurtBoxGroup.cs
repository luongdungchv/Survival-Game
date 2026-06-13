using System.Collections.Generic;
using BoxHead2.Actor;
using Dacodelaac.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoxHead2.Skills
{
    public class HurtBoxGroup : BaseMono
    {
        public HashSet<IDamageTaker> DamageTakers { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            DamageTakers = new HashSet<IDamageTaker>();
            foreach (var hurtBox in GetComponentsInChildren<HurtBox>())
            {
                hurtBox.Group = this;
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void FlipX()
        {
            foreach (var hurtBox in GetComponentsInChildren<HurtBox>())
            {
                var pos = hurtBox.Transform.localPosition;
                pos.x = -pos.x;
                hurtBox.Transform.localPosition = pos;
                EditorUtility.SetDirty(hurtBox.Transform);
            }
        }

        [Sirenix.OdinInspector.Button]
        public void AutoName()
        {
            var boxes = GetComponentsInChildren<HurtBox>();
            for (var i = 0; i < boxes.Length; i++)
            {
                boxes[i].name = $"hurt_box_{i + 1}";
                EditorUtility.SetDirty(boxes[i].gameObject);
            }
        }
#endif
    }
}