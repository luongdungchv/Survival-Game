using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FuelSlotUI : MonoBehaviour, IPointerClickHandler
{
    public static FuelSlotUI ins;
    [SerializeField] private RawImage icon;
    [SerializeField] private TextMeshProUGUI quantityText;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private TransformerBase currentTransformer => TransformerBase.currentOpen;
    private int quantity => currentTransformer.fuelSlot.quantity;
    private void Awake()
    {
        ins = this;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (iih.isItemMoving)
        {
            if (!(iih.movingItem.movingItem is IFuel)) return;
            var moving = iih.movingItem;
            var movingName = moving.movingItem.itemName;

            if (currentTransformer.fuelSlot.fuel == null || currentTransformer.fuelSlot.quantity == 0)
            {
                if (!(moving.movingItem is IFuel)) return;

                currentTransformer.SetFuel(moving.movingItem as IFuel, moving.quantity);
                Inventory.ins.Remove(movingName, moving.quantity);
                moving.quantity = 0;
                CheckIconVisibility();
                icon.texture = moving.icon;
            }

            var fuelItem = currentTransformer.fuelSlot.fuel;
            if (movingName == (fuelItem as Item).itemName)
            {
                var odd = currentTransformer.AddFuel(fuelItem, moving.quantity);
                Inventory.ins.Remove(moving.movingItem.itemName, moving.quantity - odd);
                moving.InitReplaceAction(movingName, odd);
                CheckIconVisibility();
            }
        }
        else
        {
            if (currentTransformer.fuelSlot == null || currentTransformer.fuelSlot.quantity == 0) return;
            var fuelItem = currentTransformer.fuelSlot.fuel as Item;
            var fuelQuantity = currentTransformer.fuelSlot.quantity;
            iih.movingItem.InitReplaceAction(fuelItem.itemName, fuelQuantity);
            currentTransformer.RetrieveFuel(fuelQuantity);
            CheckIconVisibility();
        }
    }
    public void CheckIconVisibility()
    {
        if (currentTransformer == null || currentTransformer.fuelSlot == null)
        {
            icon.gameObject.SetActive(false);
            return;
        };
        if (quantity <= 0) icon.gameObject.SetActive(false);
        else
        {
            icon.gameObject.SetActive(true);
            icon.texture = (currentTransformer.fuelSlot.fuel as Item).icon;
            quantityText.text = quantity.ToString();
        }
    }


}
