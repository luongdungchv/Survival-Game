using System.Collections;
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
    //private InventorySlotUI _sourceItem;
    private Inventory inventory => Inventory.ins;
    public ItemMoveIcon movingItem => _movingItem;
    public bool isItemMoving => _movingItem.gameObject.activeSelf;

    public InventoryInteractionHandler()
    {
        currentOpen = this;

        OnInit.AddListener(Init);
    }
    private void Start()
    {

    }
    private void Awake()
    {
        // if (slots == null || slots.Count == 0) InitSlots();
        // UpdateUI();
    }
    private void OnEnable()
    {

    }
    private void Update()
    {

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
        // if (quantity <= 0)
        // {
        //     _movingItem.gameObject.SetActive(false);
        // }
    }
    public void ChangeSourceItem(InventorySlotUI newSource)
    {
        //this._sourceItem = newSource;
        this._movingItem.sourceSlot = newSource;
    }
    public void UpdateUI()
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (i >= slots.Count) break;
            var itemVal = inventory.items[i];
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
}
