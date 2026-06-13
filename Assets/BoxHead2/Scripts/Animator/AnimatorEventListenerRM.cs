using UnityEngine;
#if UNITY_EDITOR
using Vector3 = UnityEngine.Vector3;
#endif
namespace BoxHead2.AnimatorEventCustom
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class AnimatorEventListenerRM : AnimatorEventListener
    {
        void OnAnimatorMove()
        {
            TriggerAnimatorMoveEvent();
        }
#if UNITY_EDITOR
        void LateUpdate()
        {
            if (Application.isPlaying) return;
            transform.localPosition = Vector3.zero;
        }
#endif
    }
}