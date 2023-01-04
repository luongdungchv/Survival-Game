using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMapper : MonoBehaviour
{
    public static ObjectMapper ins;
    [SerializeField] private NetworkPrefab[] prefabs;
    private Dictionary<NetworkPrefab, int> map;
    private void Awake()
    {
        ins = this;
        DontDestroyOnLoad(this.gameObject);

        map = new Dictionary<NetworkPrefab, int>();
        for (int i = 0; i < prefabs.Length; i++)
        {
            map.Add(prefabs[i], i);
        }
    }
    public NetworkPrefab GetPrefab(int index)
    {
        if (index < 0 || index >= prefabs.Length) return null;
        return prefabs[index];
    }
    public int GetPrefabIndex(NetworkPrefab prefab)
    {
        if (map.ContainsKey(prefab)) return map[prefab];
        return -1;
    }
}

