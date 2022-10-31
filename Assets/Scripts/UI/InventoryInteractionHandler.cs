using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInteractionHandler : MonoBehaviour
{
    public static InventoryInteractionHandler ins;
    [SerializeField] private ItemMoveIcon _movingItem;
    [SerializeField] private List<InventorySlotUI> slots;
    [SerializeField] private Transform equipmentContainer, bag;
    //private InventorySlotUI _sourceItem;
    private Inventory inventory => Inventory.ins;
    public int movingCount => _movingItem.quantity;
    public Texture2D movingIcon => _movingItem.icon;
    public int sourceItemIndex => _movingItem.sourceSlot.itemIndex;
    public int sourceItemCount => _movingItem.sourceSlot.quantity;
    public Item sourceItem => _movingItem.movingItem;
    public bool isItemMoving => _movingItem.gameObject.activeSelf;
    // Start is called before the first frame update
    // private void Awake()
    // {
    //     ins = this;
    // }
    public InventoryInteractionHandler()
    {
        ins = this;
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
        // this._sourceItem = source;
        // _movingItem.quantity = this._sourceItem.quantity;
        // this._sourceItem.quantity = 0;
        // _movingItem.icon = this._sourceItem.icon;
        _movingItem.quantity = source.quantity;
        source.quantity = 0;
        _movingItem.icon = source.icon;
        _movingItem.sourceIndex = source.itemIndex;
        _movingItem.movingItem = inventory.GetItem(source.itemIndex);
        _movingItem.sourceSlot = source;
    }
    public void ChangeMoveIconQuantity(int quantity)
    {
        _movingItem.quantity = quantity;
        if (quantity <= 0)
        {
            _movingItem.gameObject.SetActive(false);
        }
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
        }
    }
    private void InitSlots()
    {
        this.slots = new List<InventorySlotUI>();
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
