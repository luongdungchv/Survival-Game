using System;
using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Combat;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/BuffAction")]
    public class BuffAction : SubAction
    {
        [SerializeField] BuffData[] buffs;
        [SerializeField] float duration = 10f;
        [Header("Feedback")]
        [SerializeField] Feedback buffFeedback;

        public float Duration => duration;

         IActor _actor;

        public override T CreateCopy<T>()
        {
            var copy = base.CreateCopy<BuffAction>();
            copy.buffs = new BuffData[buffs.Length];
            for (var i = 0; i < copy.buffs.Length; i++)
            {
                copy.buffs[i] = buffs[i].CreateCopy();
            }

            return copy as T;
        }

        public override void Trigger(object target, IActor actor)
        {
            _actor = actor;
            foreach (var buff in buffs)
            {
                actor.AddBuff(buff);
            }
            if (Duration > 0)
            {
                StartPerformRoutine();
            }
            if (buffFeedback)
            {
                buffFeedback.Play();
            }
        }

        void StopBuff()
        {
            if (_actor != null)
            {
                foreach (var buff in buffs)
                {
                    _actor.RemoveBuff(buff);
                }
            }
        }

        public override void Detach(object target)
        {
            StopPerformRoutine();
            StopBuff();
        }

        public override void Attach(object target)
        {
            StopBuff();
        }

        protected override IEnumerator IEPerform()
        {
            yield return new WaitForSeconds(Duration);
            StopBuff();
        }
    }

    [Serializable]
    public class BuffData
    {
        [SerializeField] public BuffType type;
        [SerializeField] int attackId;
        [SerializeField] DamageConfigModify damageModify;
        
        public BuffData CreateCopy()
        {
            var copy = new BuffData()
            {
                type = type,
                damageModify = damageModify
            };
            return copy;
        }

        public DamageConfig ModifyDamageConfig(DamageConfig damageConfig)
        {
            switch (type)
            {
                case BuffType.None:
                    break;
            }
            return damageConfig;
        }
        
        public enum BuffType
        {
            None,
        }
    }
}