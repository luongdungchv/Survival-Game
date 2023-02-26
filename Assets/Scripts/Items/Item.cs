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
    public static void ClearItems(){
        itemMapper.Clear();
    }
    public string itemName;
    public string dropDisplayName;
    public GameObject dropPrefab;
    public Texture2D dropTexture;
    public Color dropOutlineColor;
    public Texture2D icon;
    public bool stackable = true;
    protected virtual void Awake()
    {
        if(itemMapper.ContainsKey(this.itemName))
            itemMapper.Remove(this.itemName);
        itemMapper.Add(itemName, this);
    }
    public ItemDrop Drop(Vector3 dropPos, int quantity)
    {
        if (quantity <= 0) return null;
        if (!stackable) quantity = 1;
        ItemDrop drop = null;

        var objSpawnId = ObjectMapper.ins.GetPrefabIndex(dropPrefab.GetComponent<NetworkPrefab>());
        var dropPacket = new ItemDropPacket()
        {
            objSpawnId = objSpawnId,
            spawnPos = dropPos,
            itemBase = this.itemName,
            quantity = quantity,
            objId = "0",
        };

        if (Client.ins.isHost)
        {
            var dropNetObj = Instantiate(dropPrefab, dropPos, Quaternion.identity).GetComponent<NetworkSceneObject>();
            dropNetObj.GenerateId();

            drop = dropNetObj.GetComponentInChildren<ItemDrop>();
            
            drop.displayName = this.dropDisplayName;
            drop.SetMaterial(this.dropTexture, this.dropOutlineColor);
            
            drop.gameObject.SetActive(true);
            drop.SetQuantity(quantity);
            drop.SetBase(this);
            dropPacket.objId = dropNetObj.id;
        }
        Client.ins.SendTCPPacket(dropPacket);
        return drop;
    }
}
public interface IUsable
{
    void OnUse(int itemIndex);
    void OnUse(NetworkPlayer netUser);
}
public interface IConsumable
{
    float duration { get; }
    void OnConsume(int itemIndex);
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
public interface ITransformable
{
    Item goalItem { get; }
    // Total units required to cook 1 item
    int cookability { get; }
}
public interface IFuel
{
    // Times taken to cook 1 unit
    float efficiency { get; }
    // Number of units
    int durability { get; }
}
[System.Serializable]
public class MaterialList
{
    public string name;
    public int quantity;
}

