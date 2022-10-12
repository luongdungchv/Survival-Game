using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemDataMapper : MonoBehaviour
{
    public static ItemDataMapper ins;
    [SerializeField]
    private List<ItemDataMapperField> itemTypeList;
    private Dictionary<string, ItemDataMapperField> itemTypeDict;

    private void Awake()
    {
        ins = this;
        itemTypeDict = new Dictionary<string, ItemDataMapperField>();
        foreach (var i in itemTypeList)
        {
            itemTypeDict.Add(i.name, i);
        }
    }
    public ItemDataMapperField GetItemData(string name) => itemTypeDict[name];

}
[System.Serializable]
public class ItemDataMapperField
{
    public string name;
    public GameObject spawnAfterDrop, spawnWhenHolding;
    public Texture2D icon;
    public UnityEvent OnUse;
}

