using UnityEngine;

namespace Dacodelaac.Core
{
    [CreateAssetMenu(menuName = "Identity")]
    public class Identity : BaseSO
    {
        public event System.Action OnChangedEvent;

        object _value;

        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                OnChangedEvent?.Invoke();
            }
        }
    }
}