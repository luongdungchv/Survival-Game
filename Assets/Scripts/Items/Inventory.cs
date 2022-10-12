using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory ins;
    [SerializeField] private int _maxInventorySlot;
    public int maxInventorySlot => _maxInventorySlot;
    [SerializeField] private ItemData testItem;
    public ItemSlot[] items;
    private Dictionary<string, List<Vector2Int>> itemQuantities;


    private void Start()
    {
        ins = this;
        items = new ItemSlot[15];
        itemQuantities = new Dictionary<string, List<Vector2Int>>();
        Add(testItem, 50);
        Add(testItem, 10);
        Add(testItem, 14);
        Add(testItem, 28);
    }

    public bool Add(ItemData itemData, int quantity, bool stackable = true)
    {
        if (quantity > maxInventorySlot) return false;

        if (itemQuantities.ContainsKey(itemData.itemName))
        {

            var propList = new List<Vector2Int>(itemQuantities[itemData.itemName]);
            propList.Sort((a, b) => a.x < b.x ? -1 : (a.x == b.x ? 0 : 1));
            var leastQuantity = propList[0];
            leastQuantity.x += quantity;
            if (leastQuantity.x > maxInventorySlot)
            {
                var redundant = leastQuantity.x - maxInventorySlot;
                leastQuantity.x = maxInventorySlot;

                for (int i = 0; i < items.Length; i++)
                {
                    //Debug.Log(i);
                    if (items[i] == null)
                    {
                        items[i] = new ItemSlot(redundant, itemData);
                        itemQuantities[itemData.itemName].Add(new Vector2Int(redundant, i));
                        break;
                    }
                    if (items[i] == null && i == items.Length - 1) return false;
                }
            }
            items[leastQuantity.y].quantity = leastQuantity.x;
            itemQuantities[itemData.itemName][0] = leastQuantity;
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = new ItemSlot(quantity, itemData);
                    itemQuantities.Add(itemData.itemName, new List<Vector2Int> { new Vector2Int(quantity, i) });
                    break;
                }
                if (items[i] == null && i == items.Length - 1) return false;
            }
        }

        return true;

    }
    public void Move(int startIndex, int startQuantity, int endIndex, int endQuantity)
    {
        Debug.Log($"{startIndex} {startQuantity} {endIndex} {endQuantity}");
        var itemData = items[startIndex].itemData;
        if (startQuantity != 0) items[startIndex].quantity = startQuantity;
        else items[startIndex] = null;

        if (endQuantity != 0) items[endIndex] = new ItemSlot(endQuantity, itemData);
        else items[endIndex] = null;
    }
    //[System.Serializable]
    public class ItemSlot
    {
        public int quantity;
        public ItemData itemData;
        public ItemSlot(int quantity, ItemData data)
        {
            this.quantity = quantity;
            this.itemData = data;
        }
    }
    [System.Serializable]
    class Test
    {
        public string name;
        public List<Vector2Int> pos;
        public Test(string name, List<Vector2Int> pos)
        {
            this.name = name;
            this.pos = pos;
        }
    }
}
