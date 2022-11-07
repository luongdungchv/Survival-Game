using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
    //public GameObject inHandModel;
    public Texture2D icon;
    public bool stackable = true;
    protected virtual void Awake()
    {
        itemMapper.Add(itemName, this);
    }
    public ItemDrop Drop(Vector3 dropPos, int quantity)
    {
        if (quantity <= 0) return null;
        if (!stackable) quantity = 1;
        var drop = Instantiate(dropPrefab, dropPos, Quaternion.identity).GetComponentInChildren<ItemDrop>();
        drop.gameObject.SetActive(true);
        drop.SetQuantity(quantity);
        drop.SetBase(this);
        return drop;
    }
}
public interface IUsable
{
    void OnUse(int itemIndex);
}
public interface IConsumable
{
    float duration { get; set; }
    void OnConsume();
}
public interface IEquippable
{
    GameObject inHandModel { get; }
    void OnEquip();
    void OnUnequip();
}
public interface ICraftable
{
    Dictionary<string, int> requiredMats { get; }
}
[System.Serializable]
public class MaterialList
{
    public string name;
    public int quantity;
}

