using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dacodelaac.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
// using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;


namespace Dacodelaac.ObjectPooling
{
    [CreateAssetMenu(menuName = "ObjectPooling/Pools")]
    public class Pools : BaseSO, ISerializationCallbackReceiver
    {
        [SerializeField] PoolData[] poolDatas;
        [SerializeField] PoolData[] scenePoolDatas;

        Dictionary<GameObject, Queue<GameObject>> waitPool;
        LinkedList<GameObject> activePool;

        Transform container;
        bool initialized;

        public override void Initialize()
        {
            if (initialized) return;
            initialized = true;
            
            waitPool = new Dictionary<GameObject, Queue<GameObject>>();
            activePool = new LinkedList<GameObject>();
            container = new GameObject("Pool").transform;
            DontDestroyOnLoad(container.gameObject);

            PreSpawn();
        }

        public bool Contains(GameObject gameObject)
        {
            var id = gameObject.GetComponent<PooledObjectId>();
            if (id == null)
            {
                return false;
            }
            if (!waitPool.ContainsKey(id.prefab))
            {
                return false;
            }
            return waitPool[id.prefab].Contains(gameObject);
        }

        void PreSpawn()
        {
            foreach (var data in poolDatas)
            {
                for (var i = 0; i < data.preSpawn; i++)
                {
                    SpawnNew(data.prefab);
                }
            }
        }

        public void SpawnOnScene()
        {
            foreach (var data in scenePoolDatas)
            {
                for (var i = 0; i < data.preSpawn; i++)
                {
                    SpawnNew(data.prefab);
                }
            }
        }

        public void SpawnNew(GameObject prefab)
        {
            var gameObject = Instantiate(prefab);
            var id = gameObject.AddComponent<PooledObjectId>();
            id.prefab = prefab;
            
            activePool.AddLast(gameObject);
            
            Despawn(gameObject, false);
        }

        public void Despawn(GameObject gameObject, bool destroy = false)
        {
            if (gameObject == null) return;
            
            if (pools.Contains(gameObject)) return;
            
            var id = gameObject.GetComponent<PooledObjectId>();
            if (id == null)
            {
                Debug.LogError($"{gameObject.name} is not a pooled object!");
                return;
            }
            if (!activePool.Contains(gameObject))
            {
                Debug.LogError($"{gameObject.name} is not in active pool!");
                return;
            }
            var children = gameObject.GetComponentsInChildren<PooledObjectId>(true);
            if (children.Length > 0)
            {
                foreach (var child in children)
                {
                    if (child != id)
                    {
                        Despawn(child.gameObject, destroy);
                    }
                }
            }
            activePool.Remove(gameObject);
            if (!waitPool.ContainsKey(id.prefab))
            {
                waitPool.Add(id.prefab, new Queue<GameObject>());
            }
            var stack = waitPool[id.prefab];
            if (stack.Contains(gameObject))
            {
                Debug.LogError($"{gameObject.name} is already pooled!");
                return;
            }
            CleanUp(gameObject);
            if (destroy)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
                if (container != null) // fix error spawn new object when closing scene
                {
                    gameObject.transform.SetParent(container, false);   
                }
                stack.Enqueue(gameObject);
            }
        }

        public void DespawnAll()
        {
            var arr = activePool.ToArray();
            foreach (var o in arr)
            {
                Despawn(o);
            }
        }

        /// <summary>
        /// Call after DespawnAll()
        /// </summary>
        public void DestroyAll()
        {
            var arr = waitPool.Values.SelectMany(g => g).Where(g => g != null).ToArray();
            for (var i = 0; i < arr.Length; i++)
            {
#if UNITY_EDITOR && DACODER_LOG
                arr[i].gameObject.GetComponent<PooledObjectId>().PreparedForDestroy = true;
#endif
            }
            waitPool.Clear();
            __.Ticker?.StartCoroutine(CleanUpGo());
        }

        IEnumerator CleanUpGo()
        {
            yield return null;
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public T Spawn<T>(T type, Transform parent = null, bool initialize = true) where T : Component
        {
            return Spawn(type.gameObject, parent, initialize).GetComponent<T>();
        }
        
        public GameObject Spawn(GameObject prefab, Transform parent = null, bool initialize = true)
        {
            Initialize();
            if (!waitPool.ContainsKey(prefab))
            {
                waitPool.Add(prefab, new Queue<GameObject>());
            }

            var stack = waitPool[prefab];
            if (stack.Count == 0)
            {
                SpawnNew(prefab);
            }
            var gameObject = stack.Dequeue();
            
            gameObject.transform.SetParent(parent, false);

            if (parent == null)
            {
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            }

            gameObject.SetActive(true);
            
            if (initialize)
            {
                Initialize(gameObject);
            }

            activePool.AddLast(gameObject);
            
            return gameObject;
        }

        public void Initialize(GameObject go)
        {
            var monos = go.GetComponentsInChildren<BaseMono>(true);
            foreach (var mono in monos)
            {
                mono.Initialize();
            }
        }

        public void CleanUp(GameObject go)
        {
            var monos = go.GetComponentsInChildren<BaseMono>(true);
            foreach (var mono in monos)
            {
                mono.CleanUp();
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            initialized = false;
        }
        
    }

    [Serializable]
    public class PoolData
    {
        public GameObject prefab;
        public int preSpawn;
    }
}
