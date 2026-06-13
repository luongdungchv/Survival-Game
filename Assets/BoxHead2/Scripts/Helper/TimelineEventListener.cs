using System;
using Dacodelaac.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace BoxHead2.Helper
{
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelineEventListener : BaseMono
    {
        public event Action OnStartEvent;
        public event Action OnFinishEvent;
        public void OnStart()
        {
            OnStartEvent?.Invoke();
        }

        public void OnFinish()
        {
            OnFinishEvent?.Invoke();
        }
    }
}