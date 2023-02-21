﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory ins;
    [SerializeField] private int _maxInventorySlot, equipSlotCount;
    [SerializeField] private InitialItemData[] initialItems;
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
    [SerializeField] private Transform dropPos;
    public int maxInventorySlot => _maxInventorySlot;
    public ItemSlot[] items;
    private Dictionary<string, int> itemQuantities;
    private StateMachine fsm;
    InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;

    private void Awake()
    {
        ins = this;
        items = new ItemSlot[28];
        itemQuantities = new Dictionary<string, int>();
        //iih.Init();
        InventoryInteractionHandler.InitAllInstances();
        // foreach(var i in initialItems){
        //     this.Add(i.item, i.quantity);
        // }
        Add("knife", 1);
        Add("sus_shroom", 1);
        Add("craft_table", 1);
        Add("furnace", 1);
        Add("anvil", 1);
        Add("oak_wood", 12);
        Add("mithril_sword", 1);
    }
    private void Start()
    {
        ReloadInHandModel(true);
        fsm = NetworkPlayer.localPlayer.GetComponent<StateMachine>();
    }

    private void Update()
    {
        var mouseScroll = InputReader.ins.MouseScroll();
        var state = fsm.currentState.name;
        if (mouseScroll != 0 && (state == "Idle" || state == "Move" || state == "Dash" || state == "Sprint"))
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
    public int Remove(int itemIndex, int quantity)
    {
        if (itemIndex < 0) return -1;
        var item = items[itemIndex];
        if (item == null || item.itemData == null) return -1;
        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            if (item.itemData is IEquippable) (item.itemData as IEquippable).OnUnequip();
            items[itemIndex] = null;
        }
        iih?.UpdateUI();
        ReloadInHandModel(true);
        return item.quantity;
    }
    public Item GetSlotItem(int itemIndex)
    {
        var itemSlot = items[itemIndex];
        if (itemSlot == null) return null;
        return itemSlot.itemData;
    }
    public int GetSlotQuantity(int itemIndex){
        var itemSlot = items[itemIndex];
        if (itemSlot == null) return -1;
        return itemSlot.quantity;
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
    private void ReloadInHandModel(bool skip = false)
    {
        if (PlayerEquipment.ins.currentEquipIndex != _currentEquipIndex || skip)
        {
            for (int i = 0; i < equipSlotCount; i++)
            {
                if (i == _currentEquipIndex)
                {
                    Debug.Log(_currentEquipIndex);
                    iih.GetUISlot(i).Highlight(true);
                    if (items[i] == null || items[i].itemData == null)
                    {
                        PlayerEquipment.ins.rightHandItem = null;
                        var packet = new UpdateEquippingPacket();
                        packet.WriteData(Client.ins.clientId, "0");
                        Client.ins.SendTCPPacket(packet);
                        continue;
                    }

                    var equippableItem = items[i].itemData as IEquippable;
                    equippableItem.OnEquip();
                    PlayerEquipment.ins.rightHandItem = items[i].itemData;
                }
            }
        }
        PlayerEquipment.ins.currentEquipIndex = _currentEquipIndex;
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
    public bool DropItem(int itemIndex, int quantity, Vector3 dropPostion)
    {
        var dropSlot = items[itemIndex];
        if (dropSlot == null) return false;
        dropSlot.quantity -= quantity;
        if(dropSlot.itemData is IEquippable) 
            (dropSlot.itemData as IEquippable).OnUnequip();
        dropSlot.itemData.Drop(dropPostion, quantity);
        if (dropSlot.quantity <= 0)
        {
            items[itemIndex] = null;
        }
        Debug.Log(items[itemIndex]);
        ReloadInHandModel(true);
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
    class InitialItemData
    {
        public Item item;
        public int quantity;
    }
}
