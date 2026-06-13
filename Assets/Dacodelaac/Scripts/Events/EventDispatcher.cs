using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.Events
{
    public class EventDispatcher : BaseMono
    {
        [SerializeField] Event @event;
        [SerializeField] bool dispatchOnEnable;

        public override void Initialize()
        {
            if (dispatchOnEnable)
            {
                Dispatch();
            }
        }

        public void Dispatch()
        {
            @event.Raise();
        }
    }
}