﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CookOutputSlotUI : MonoBehaviour, IPointerClickHandler
{
    public static CookOutputSlotUI ins;

    [SerializeField] private RawImage icon;

    [SerializeField] private TextMeshProUGUI quantityText;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private void Awake()
    {
        ins = this;
    }

    public void CheckIconVisibility()
    {
        var currentCooker = Transformer.currentOpen;
        if (currentCooker.outputSlot.item == null || currentCooker.outputSlot.quantity <= 0)
        {
            icon.gameObject.SetActive(false);
            return;
        }
        if (currentCooker.outputSlot.quantity > 0)
        {
            icon.gameObject.SetActive(true);
            icon.texture = currentCooker.outputSlot.item.icon;
            quantityText.text = currentCooker.outputSlot.quantity.ToString();
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var currentCooker = Transformer.currentOpen;
        if (iih.isItemMoving || currentCooker.outputSlot == null || currentCooker.outputSlot.quantity == 0)
            return;
        var quantity = currentCooker.outputSlot.quantity;
        iih.movingItemHolder.InitReplaceAction(currentCooker.outputSlot.item.itemName, quantity);
        currentCooker.RetrieveOutput(quantity);
        CheckIconVisibility();
    }
}
