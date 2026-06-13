using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Actor
{
    [CreateAssetMenu(menuName = "Feedback/ActorFeedback")]
    public class ActorFeedback : BaseSO
    {
        [SerializeField] public Feedback appearFeedback;
        [SerializeField] public Feedback idleFeedback;
        [SerializeField] public Feedback footFeedback;
        [SerializeField] public Feedback jumpFeedback;
        [SerializeField] public Feedback jumpLandingFeedback;
        [SerializeField] public Feedback getHitFeedback;
        [SerializeField] public Feedback deadFeedback;
        [SerializeField] public Feedback staggerFeedback;
        [SerializeField] public Feedback rollFeedback;
        [SerializeField] public Feedback reviveFeedback;
        [SerializeField] public Feedback healFeedback;
        [SerializeField] public Feedback stealthEnterFeedback;
        [SerializeField] public Feedback stealthExitFeedback;
    }
}
