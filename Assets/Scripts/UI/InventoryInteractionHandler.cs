using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInteractionHandler : MonoBehaviour
{
    public static InventoryInteractionHandler ins;
    [SerializeField] private ItemMoveIcon _movingItem;
    [SerializeField] private List<InventorySlotUI> slots;
    private InventorySlotUI _sourceItem;
    private Inventory inventory => Inventory.ins;
    public int movingCount => _movingItem.quantity;
    public Texture2D movingIcon => _movingItem.icon;
    public int sourceItemIndex => _sourceItem.itemIndex;
    public int sourceItemCount => _sourceItem.quantity;
    public bool isItemMoving => _movingItem.gameObject.activeSelf;
    // Start is called before the first frame update
    private void Start()
    {
        ins = this;
    }
    private void OnEnable()
    {
        UpdateUI();
    }
    private void Update()
    {
        if (isItemMoving && Input.mouseScrollDelta.y != 0)
        {
            _movingItem.quantity += (int)Input.mouseScrollDelta.y;
            _sourceItem.quantity -= (int)Input.mouseScrollDelta.y;

            int baseItemQuantity = inventory.items[_sourceItem.itemIndex].quantity;
            _sourceItem.quantity = Mathf.Clamp(_sourceItem.quantity, 0, Mathf.Min(baseItemQuantity, inventory.maxInventorySlot) - 1);
            _movingItem.quantity = Mathf.Clamp(_movingItem.quantity, 1, Mathf.Min(baseItemQuantity, inventory.maxInventorySlot));
        }
    }
    public void SetMovingItem(InventorySlotUI source)
    {
        _movingItem.gameObject.SetActive(true);
        this._sourceItem = source;
        _movingItem.quantity = this._sourceItem.quantity;
        this._sourceItem.quantity = 0;
        _movingItem.icon = this._sourceItem.icon;
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
        this._sourceItem = newSource;
    }
    private void UpdateUI()
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            var itemVal = inventory.items[i];
            if (i < slots.Count) slots[i].itemIndex = i;
            if (itemVal != null)
            {
                slots[i].quantity = itemVal.quantity;
                slots[i].icon = itemVal.itemData.icon;
            }
        }
    }
}
