using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory ins;
    [SerializeField] private int _maxInventorySlot, equipSlotCount;
    private int _currentEquipIndex;
    public int currentEquipIndex
    {
        get => _currentEquipIndex;
        set
        {
            _currentEquipIndex = value;
            _currentEquipIndex = Mathf.Clamp(currentEquipIndex, 0, equipSlotCount - 1);

            ReloadInHandModel();
        }
    }
    [SerializeField] private Item testItem, testItem2, testItem3;
    [SerializeField] private Transform dropPos;
    public int maxInventorySlot => _maxInventorySlot;
    public ItemSlot[] items;
    private Dictionary<string, int> itemQuantities;
    InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;

    private void Awake()
    {
        ins = this;
        items = new ItemSlot[28];
        itemQuantities = new Dictionary<string, int>();
        //iih.Init();
        InventoryInteractionHandler.InitAllInstances();
        Add("furnace", 1);
        Add("craft_table", 1);
        Add(testItem, 1);
        Add(testItem2, 64);
        Add(testItem3, 64);
        Add("gold_ore", 64);
        Add("sus_shroom", 12);
        Add("knife", 1);
        //Debug.Log(Remove(testItem2.itemName, 64));

    }
    private void Start()
    {
        ReloadInHandModel();
        // Add(testItem, 50);
        // Add(testItem, 10);
        // Add(testItem, 14);
        // Add(testItem, 28);

        //Add(testItem, 1);

    }

    private void Update()
    {
        var mouseScroll = InputReader.ins.MouseScroll();
        if (mouseScroll != 0)
        {
            currentEquipIndex += mouseScroll;
        }
        if (InputReader.ins.inputNum != -1)
        {
            currentEquipIndex = InputReader.ins.inputNum - 1;
        }

    }
    public bool isEquipSlot(int index) => index < equipSlotCount;
    public bool Add(Item itemData, int quantity)
    {
        //Debug.Log(items.Length);
        if (quantity > maxInventorySlot || quantity == 0 || itemData == null) return false;
        bool stackable = itemData.stackable;

        int nullIndex = -1;
        bool equippable = itemData is IEquippable;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                if (!equippable && i < equipSlotCount) continue;
                nullIndex = i;
                break;
            }
        }

        if (!stackable)
        {
            if (nullIndex == -1 || quantity != 1) return false;
            items[nullIndex] = new ItemSlot(1, itemData);
            ReloadInHandModel();
            iih?.UpdateUI();
            return true;

        }

        List<Vector2Int> itemSlotList = new List<Vector2Int>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemData.itemName == itemData.itemName && items[i].quantity < maxInventorySlot)
            {
                itemSlotList.Add(new Vector2Int(items[i].quantity, i));
            }
        }

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
            if (nullIndex == -1) return false;
            items[nullIndex] = new ItemSlot(quantity, itemData);
        }
        foreach (var i in itemSlotList)
        {
            items[i.y].quantity = i.x;
        }
        ReloadInHandModel();
        iih?.UpdateUI();
        return true;

    }
    public bool Add(string itemName, int quantity)
    {
        return Add(Item.GetItem(itemName), quantity);
    }
    public bool Replace(Item item, int quantity, int slotIndex)
    {
        items[slotIndex] = new ItemSlot(quantity, item);

        return true;
    }
    public bool Move(int startIndex, int startQuantity, int endIndex, int endQuantity)
    {
        var itemData = items[startIndex].itemData;
        bool equippable = itemData is IEquippable;
        if (!equippable && endIndex < equipSlotCount) return false;

        if (startQuantity != 0) items[startIndex].quantity = startQuantity;
        else items[startIndex] = null;

        if (endQuantity != 0) items[endIndex] = new ItemSlot(endQuantity, itemData);
        else items[endIndex] = null;
        ReloadInHandModel();
        iih?.UpdateUI();
        return true;
    }
    public bool Remove(string itemName, int quantity)
    {
        List<Vector2Int> itemSlotList = new List<Vector2Int>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemData.itemName == itemName && items[i].quantity <= maxInventorySlot)
            {
                itemSlotList.Add(new Vector2Int(items[i].quantity, i));
            }
        }
        for (int i = 0; i < itemSlotList.Count; i++)
        {
            var tmp = itemSlotList[i];
            tmp.x -= quantity;
            if (tmp.x < 0)
            {
                quantity = 0 - tmp.x;
                tmp.x = 0;
            }
            else quantity = 0;
            itemSlotList[i] = tmp;
        }
        if (quantity > 0) return false;
        foreach (var i in itemSlotList)
        {
            if (i.x == 0)
            {
                items[i.y] = null;
                continue;
            }
            items[i.y].quantity = i.x;
        }
        iih?.UpdateUI();
        return true;
    }
    public bool Remove(int itemIndex, int quantity)
    {
        var item = items[itemIndex];
        if (item == null || item.itemData == null) return false;
        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            if (item.itemData is IEquippable) (item.itemData as IEquippable).OnUnequip();
            items[itemIndex] = null;
        }
        iih?.UpdateUI();
        ReloadInHandModel();
        return true;
    }
    public Item GetItem(int itemIndex)
    {
        var itemSlot = items[itemIndex];
        if (itemSlot == null) return null;
        return itemSlot.itemData;
    }
    public int GetItemQuantity(string itemName)
    {
        int res = 0;
        foreach (var i in items)
        {
            if (i == null || i.itemData == null || i.itemData.itemName != itemName) continue;
            res += i.quantity;
        }
        return res;
    }
    private void ReloadInHandModel()
    {
        PlayerEquipment.ins.currentEquipIndex = _currentEquipIndex;
        for (int i = 0; i < equipSlotCount; i++)
        {
            if (i == _currentEquipIndex)
            {
                iih.GetUISlot(i).Highlight(true);
                if (items[i] == null || items[i].itemData == null)
                {
                    PlayerEquipment.ins.rightHandItem = null;
                    continue;
                }

                var equippableItem = items[i].itemData as IEquippable;
                equippableItem.OnEquip();
                PlayerEquipment.ins.rightHandItem = items[i].itemData;

            }
        }
        for (int i = 0; i < equipSlotCount; i++)
        {
            if (i != _currentEquipIndex)
            {
                iih.GetUISlot(i).Highlight(false);
                if (items[i] == null || items[i].itemData == null)
                {
                    continue;
                }
                var equippableItem = items[i].itemData as IEquippable;
                var currentModel = (PlayerEquipment.ins.rightHandItem as IEquippable)?.inHandModel;
                if (currentModel == null || equippableItem.inHandModel != currentModel)
                {
                    equippableItem.OnUnequip();
                }
            }
        }
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
