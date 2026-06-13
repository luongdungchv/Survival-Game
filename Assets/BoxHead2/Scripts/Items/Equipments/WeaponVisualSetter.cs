using System;
using System.Linq;
using BoxHead2.Actor;
using BoxHead2.SubActions;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Items
{
    public class WeaponVisualSetter : BaseMono
    {
        [SerializeField] bool listenEvent;
        [SerializeField] WeaponVisual defaultVisual;

        IActor _actor;
        
        public override void ListenEvents()
        {
            base.ListenEvents();
            if (listenEvent && _actor != null)
            {
                _actor.OnBuffChangedEvent += OnChanged;
            }
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            if (listenEvent && _actor != null)
            {
                _actor.OnBuffChangedEvent -= OnChanged;
            }
        }

        public override void DoEnable()
        {
            base.DoEnable();
            OnChanged();
        }

        void Bind(IActor actor)
        {
            StopListenEvents();
            _actor = actor;
            ListenEvents();
            OnChanged();
        }

        void OnChanged()
        {
            defaultVisual.Show(true);
        }

        public static void Bind(IActor actor, GameObject target)
        {
            var weaponVisualSetters = target.GetComponentsInChildren<WeaponVisualSetter>(true);
            foreach (var visualSetter in weaponVisualSetters)
            {
                visualSetter.Bind(actor);
            }
        }
    }
    
    [Serializable]
    public class WeaponVisual
    {
        [SerializeField] GameObject[] visuals;

        public bool IsShow { get; private set; }
        
        public void Show(bool on)
        {
            IsShow = on;
            foreach (var visual in visuals)
            {
                visual.SetActive(IsShow);
            }
        }
    }
}