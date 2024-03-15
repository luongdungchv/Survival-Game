using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
// using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace DL.Utils
{
    public static class SceneUtils
    {
        public static T SpawnIntoScene<T>(T prefab, Vector3 position = default, Quaternion rotation = default, Scene targetScene = default) where T : UnityEngine.Component
        {
            var obj = GameObject.Instantiate(prefab, position, rotation);
            if (targetScene.Equals(default(Scene))) targetScene = SceneManager.GetSceneByName("Gameplay");
            SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
            return obj;
        }
        // public static async Task<T> SpawnIntoScene<T>(AssetReference prefab, Vector3 position = default, Quaternion rotation = default, Scene targetScene = default) where T : UnityEngine.Component
        // {
        //     var handle = await prefab.LoadAssetAsync<GameObject>().Task;
        //     var objPrefab = handle.GetComponent<T>();
        //     var obj = GameObject.Instantiate(objPrefab, position, rotation);
        //     if (targetScene.Equals(default(Scene))) targetScene = SceneManager.GetSceneByName("Gameplay");
        //     SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
        //     return obj;
        // }
        public static GameObject SpawnIntoScene(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Scene targetScene = default)
        {

            var obj = GameObject.Instantiate(prefab, position, rotation);
            if (targetScene.Equals(default(Scene))) targetScene = SceneManager.GetSceneByName("Gameplay");
            SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
            return obj;
        }
    }
}