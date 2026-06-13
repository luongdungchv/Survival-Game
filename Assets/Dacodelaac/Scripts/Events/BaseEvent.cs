using System.Collections.Generic;
using System.Reflection;
using Dacodelaac.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dacodelaac.Events
{
    public class BaseEvent : BaseSO, IEvent
    {
        readonly List<IEventListener> listeners = new List<IEventListener>();

        public void Raise()
        {
#if UNITY_EDITOR
            Debug.Log($"===> {name}");
#endif
            for (var i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(this);
            }
        }

        public void AddListener(IEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void RemoveListener(IEventListener listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }

        public void RemoveAll()
        {
            listeners.Clear();
        }
    }

    public class BaseEvent<TType> : BaseSO, IEvent<TType>
    {
        readonly List<IEventListener<TType>> listeners = new List<IEventListener<TType>>();

        public virtual void Raise(TType value)
        {
#if UNITY_EDITOR
            Debug.Log($"===> {name}");
#endif
            var triggered = new HashSet<IEventListener<TType>>();
            for (var i = listeners.Count - 1; i >= 0; i--)
            {
                if (i >= listeners.Count || triggered.Contains(listeners[i]))
                {
                    continue;
                }
                triggered.Add(listeners[i]);
                listeners[i].OnEventRaised(value);
            }
        }

        public void AddListener(IEventListener<TType> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void RemoveListener(IEventListener<TType> listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }

        public void RemoveAll()
        {
            listeners.Clear();
        }
    }
}