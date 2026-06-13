using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/Ads Request Show Rewarded Event")]
    public class AdsShowRewardedEvent : BaseEvent<AdsShowRewardedData>
    {
    }

    public class AdsShowRewardedData
    {
        public string AdsId;
        public System.Action OnAvailable;
        public System.Action OnNotAvailable;
        public System.Action<string, string, string> OnCompleted;
        public System.Action OnClosed;
    }
}