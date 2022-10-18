using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    public static Inventory ins;
    [SerializeField] private int _maxInventorySlot;
    [SerializeField] private Item testItem;
    [SerializeField] private Transform dropPos;
    public int maxInventorySlot => _maxInventorySlot;
    public ItemSlot[] items;
    private Dictionary<string, List<Vector2Int>> itemQuantities;


    private void Awake()
    {
        ins = this;
        items = new ItemSlot[3];
        itemQuantities = new Dictionary<string, List<Vector2Int>>();

    }
    private void Start()
    {
        Add(testItem, 50);
        Add(testItem, 10);
        Add(testItem, 14);
        Add(testItem, 28);
    }
    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.D))
    //     {
    //         Add(testItem, 40);
    //     }
    // }

    public bool Add(Item itemData, int quantity, bool stackable = true)
    {
        if (quantity > maxInventorySlot) return false;
        if (quantity == 0) return false;
        if (itemData == null) return false;

        int nullIndex = -1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                nullIndex = i;
                break;
            }
        }
        if (nullIndex == -1) return false;
        if (!stackable)
        {
            if (quantity == 1) { items[nullIndex] = new ItemSlot(1, itemData); return true; }
            else return false;
        }

        List<Vector2Int> itemSlotList = new List<Vector2Int>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemData.itemName == itemData.itemName && items[i].quantity < maxInventorySlot)
            {
                itemSlotList.Add(new Vector2Int(items[i].quantity, i));
            }
        }

        //sort with quantity
        // itemSlotList.Sort((a, b) => a.x < b.x ? -1 : (a.x > b.x ? 1 : 0));
        // itemSlotList.Reverse();
        for (int i = 0; i < itemSlotList.Count; i++)
        {
            var tmp = itemSlotList[i];
            tmp.x += quantity;
            if (tmp.x > maxInventorySlot)
            {
                quantity = tmp.x - maxInventorySlot;
                tmp.x = maxInventorySlot;
            }
            else quantity = 0;
            itemSlotList[i] = tmp;
        }
        if (quantity > 0)
        {
            items[nullIndex] = new ItemSlot(quantity, itemData);
        }
        foreach (var i in itemSlotList)
        {
            items[i.y].quantity = i.x;
        }

        return true;

    }
    public void Move(int startIndex, int startQuantity, int endIndex, int endQuantity)
    {
        var itemData = items[startIndex].itemData;
        if (startQuantity != 0) items[startIndex].quantity = startQuantity;
        else items[startIndex] = null;

        if (endQuantity != 0) items[endIndex] = new ItemSlot(endQuantity, itemData);
        else items[endIndex] = null;
    }
    public Item GetItem(int itemIndex)
    {
        return items[itemIndex].itemData;
    }
    public bool DropItem(int itemIndex, int quantity)
    {
        var dropSlot = items[itemIndex];
        if (dropSlot == null) return false;
        dropSlot.quantity -= quantity;
        dropSlot.itemData.Drop(dropPos.position, quantity);
        if (dropSlot.quantity <= 0)
        {
            items[itemIndex] = null;
        }
        return true;
    }
    //[System.Serializable]
    public class ItemSlot
    {
        public int quantity;
        public Item itemData;
        public ItemSlot(int quantity, Item data)
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
