using System;
using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/HealAction")]
    public class HealAction : SubAction
    {
        [SerializeField] Type type;
        [SerializeField] bool evenDead;
        [SerializeField] float capHpRatio = 1;
        [SerializeField] float amount;
        [SerializeField] float capHeal = 1;
        [SerializeField] float interval = -1;

        HealActionData _data;
        float _lastTimeHeal;

        public override T CreateCopy<T>()
        {
            var copy = base.CreateCopy<HealAction>();
            copy.type = type;
            copy.evenDead = evenDead;
            copy.capHpRatio = capHpRatio;
            copy.amount = amount;
            copy.capHeal = capHeal;
            copy.interval = interval;
            copy._lastTimeHeal = 0;
            copy._data = null;
            return copy as T;
        }

        public override void Trigger(object target, IActor actor)
        {
            _data = Get<HealActionData>(target);
            if (actor == null || actor.Destroyed || !actor.Alive && !evenDead || actor.HpRatio >= capHpRatio || Time.time - _lastTimeHeal < interval)
            {
                return;
            }

            var healValue = 0f;

            switch (type)
            {
                case Type.BaseOnMaxHp:
                    healValue = actor.Heal(new HealData()
                    {
                        Amount = actor.MaxHp * Mathf.Min(amount, capHeal),
                        ActiveFx = true,
                        HpText = true,
                        Revive = evenDead,
                        ReviveText = false,
                        IsPlaySound = false,
                    });
                    break;
                case Type.BaseOnDamage:
                    if (_data == null) break;
                    healValue = actor.Heal(new HealData()
                    {
                        Amount = Mathf.Min(_data.Damage * amount, actor.MaxHp * capHeal),
                        ActiveFx = true,
                        HpText = true,
                        Revive = evenDead,
                        ReviveText = false,
                        IsPlaySound = false,
                    });
                    break;
                case Type.FixedAmount:
                    healValue = actor.Heal(new HealData()
                    {
                        Amount = Mathf.Min(amount, actor.MaxHp * capHeal),
                        ActiveFx = true,
                        HpText = true,
                        Revive = evenDead,
                        ReviveText = false,
                        IsPlaySound = true,
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (healValue > 0)
            {
                _lastTimeHeal = Time.time;
            }
        }

        enum Type
        {
            BaseOnMaxHp,
            BaseOnDamage,
            FixedAmount
        }
    }

    public class HealActionData
    {
        public IActor Actor;
        public float Damage;

        public HealActionData(IActor actor, float damage = -1)
        {
            Actor = actor;
            Damage = damage;
        }
    }

    public class HealData
    {
        public float Amount;
        public bool Revive;
        public bool ActiveFx;
        public bool ReviveText;
        public bool HpText;
        public bool IsPlaySound;
    }

    public enum HealType
    {
        BaseOnDamage,
    }
}