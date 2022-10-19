using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Data", fileName = "New Item Data")]
public class Item : MonoBehaviour
{
    protected static Dictionary<string, Item> itemMapper = new Dictionary<string, Item>();
    public static Item GetItem(string name)
    {
        if (itemMapper.ContainsKey(name))
            return itemMapper[name];
        return null;
    }
    public string itemName;
    public GameObject dropPrefab;
    public GameObject inHandModel;
    public Texture2D icon;
    protected virtual void Awake()
    {
        itemMapper.Add(itemName, this);
    }
    public ItemDrop Drop(Vector3 dropPos, int quantity)
    {
        var drop = Instantiate(dropPrefab, dropPos, Quaternion.identity).GetComponentInChildren<ItemDrop>();
        drop.SetQuantity(quantity);
        drop.SetBase(this);
        return drop;
    }
}
public interface IUsable
{
    void OnUse();
}
public interface IConsumable
{
    float duration { get; set; }
    void OnConsume();
}
