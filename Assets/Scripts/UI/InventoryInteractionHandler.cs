using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryInteractionHandler : MonoBehaviour
{
    public int id;
    public static InventoryInteractionHandler currentOpen;
    private static UnityEvent OnInit = new UnityEvent();
    [SerializeField] private ItemMoveIcon _movingItem;
    [SerializeField] private List<InventorySlotUI> slots, equipSlots;
    [SerializeField] private Transform equipmentContainer, bag;
    private Inventory inventory => Inventory.ins;
    public ItemMoveIcon movingItemHolder => _movingItem;
    public bool isItemMoving => _movingItem.gameObject.activeSelf;

    public InventoryInteractionHandler()
    {
        currentOpen = this;

        OnInit.AddListener(Init);
    }
    public void Init()
    {
        if (slots == null || slots.Count == 0) InitSlots();
        UpdateUI();
    }
    public void SetMovingItem(InventorySlotUI source)
    {
        _movingItem.gameObject.SetActive(true);

        _movingItem.InitMoveAction(source);
    }
    public void ChangeMoveIconQuantity(int quantity)
    {
        _movingItem.quantity = quantity;
    }
    public void ChangeSourceItem(InventorySlotUI newSource)
    {
        this._movingItem.sourceSlot = newSource;
    }
    public void UpdateUI()
    {
        for (int i = 0; i < inventory.itemSlots.Length; i++)
        {
            if (i >= slots.Count) break;
            var itemVal = inventory.itemSlots[i];

            if (i < slots.Count) slots[i].itemIndex = i;
            if (itemVal != null)
            {
                slots[i].quantity = itemVal.quantity;
                slots[i].icon = itemVal.itemData.icon;
            }
            else
            {
                slots[i].quantity = 0;
                slots[i].icon = null;
            }
        }
    }
    public void SetAsOpen()
    {
        currentOpen = this;
        UpdateUI();
    }
    public void SetAsClose()
    {
        currentOpen = null;
    }
    public static void InitAllInstances()
    {
        OnInit.Invoke();
    }
    private void InitSlots()
    {
        this.slots = new List<InventorySlotUI>();
        this.equipSlots = new List<InventorySlotUI>();
        for (int i = 0; i < equipmentContainer.childCount; i++)
        {
            var slot = equipmentContainer.GetChild(i).GetComponent<InventorySlotUI>();
            this.slots.Add(slot);
        }
        for (int i = 0; i < bag.childCount; i++)
        {
            var slot = bag.GetChild(i).GetComponent<InventorySlotUI>();
            this.slots.Add(slot);
        }
    }
    public InventorySlotUI GetUISlot(int index)
    {
        return slots[index];
    }
    public void CheckAndDropItem()
    {
        if (movingItemHolder.sourceSlot == null && movingItemHolder.movingItem != null && movingItemHolder.quantity != 0)
        {
            movingItemHolder.movingItem.Drop(NetworkPlayer.localPlayer.transform.position + Vector3.up * 2, movingItemHolder.quantity);
        }
    }
    public void DropMovingItem()
    {
        if (isItemMoving && movingItemHolder.action == "replace")
        {
            var dropPos = NetworkPlayer.localPlayer.transform.position + Vector3.up * 5;
            movingItemHolder.movingItem.Drop(dropPos, movingItemHolder.quantity);
        }
    }
}
