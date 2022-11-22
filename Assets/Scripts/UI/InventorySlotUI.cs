using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    private int _quantity;
    public int quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            quantityText.text = _quantity.ToString();
            if (value > 0) iconImage.gameObject.SetActive(true);
            else iconImage.gameObject.SetActive(false);
        }
    }
    public int itemIndex;
    public Item item => inventory.GetItem(itemIndex);
    private Texture2D _icon;
    public Texture2D icon
    {
        get => _icon;
        set
        {
            _icon = value;
            iconImage.texture = _icon;
        }
    }
    [SerializeField] private ItemMoveIcon display;
    [SerializeField] RawImage iconImage;
    [SerializeField] TextMeshProUGUI quantityText;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private Inventory inventory => Inventory.ins;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (iih.isItemMoving)
        {
            if (inventory.isEquipSlot(itemIndex) && !(iih.movingItem.movingItem is IEquippable)) return;


            var thisItem = inventory.GetItem(itemIndex);
            var sourceItem = iih.movingItem.movingItem;

            Debug.Log(thisItem);
            if (thisItem != null && sourceItem != null && thisItem.itemName != sourceItem.itemName)
            {
                return;
            }

            if (iih.movingItem.sourceSlot == null)
            {

                var redundant = AddQuantity(iih.movingItem.quantity, iih.movingItem.icon);
                inventory.Replace(sourceItem, quantity, itemIndex);
                iih.ChangeMoveIconQuantity(redundant);
                iih.ChangeSourceItem(this);
            }
            else
            {
                var redundant = AddQuantity(iih.movingItem.quantity, iih.movingItem.icon);
                inventory.Move(iih.movingItem.sourceIndex, iih.movingItem.sourceSlot.quantity, itemIndex, quantity);
                iih.ChangeMoveIconQuantity(redundant);
                iih.ChangeSourceItem(this);
            }
        }
        else
        {
            iih.SetMovingItem(this);
        }
    }

    private int AddQuantity(int quantity, Texture2D icon)
    {
        this.quantity += quantity;
        this.icon = icon;
        int redundant = 0;
        if (this.quantity > inventory.maxInventorySlot)
        {
            redundant = this.quantity - inventory.maxInventorySlot;
            this.quantity = inventory.maxInventorySlot;
        }
        return redundant;
    }
    public void Highlight(bool mode)
    {
        var image = this.GetComponent<RawImage>();
        if (mode) image.color = Color.yellow;
        else image.color = Color.grey;

    }

}
