using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Obj Manager", menuName = "Obj Manager")]
public class ObjectManager : ScriptableObject
{
    public List<Item> itemList;
    public Dictionary<string, GameObject> map;
    private void OnEnable()
    {
        map = new Dictionary<string, GameObject>();
        foreach (var i in itemList)
        {
            map.Add(i.Id, i.prefab);
        }
    }
    [System.Serializable]
    public class Item
    {
        public string Id;
        public GameObject prefab;
    }
}


