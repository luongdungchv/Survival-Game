using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dacodelaac.Core
{
    public class InitializeLauncher : MonoBehaviour
    {
        [SerializeField] AssetReference sceneReference;

        void Start()
        {
            sceneReference.LoadSceneAsync();
        }
    }
}