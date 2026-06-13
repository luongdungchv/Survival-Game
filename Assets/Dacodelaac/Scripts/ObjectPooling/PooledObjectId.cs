using UnityEngine;

namespace Dacodelaac.ObjectPooling
{
    public class PooledObjectId : MonoBehaviour
    {
        public GameObject prefab;

#if UNITY_EDITOR && DACODER_LOG
        public bool PreparedForDestroy { get; set; }
        void OnDestroy()
        {
            if (!PreparedForDestroy)
            {
                Dacoder.LogError($"Destroying pooled object! {gameObject.name}");
            }
        }
#endif
    }
}