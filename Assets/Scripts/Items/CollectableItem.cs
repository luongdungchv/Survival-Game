using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectableItem : InteractableItem
{
    [SerializeField] private int quantity;
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool stackable;

    protected override void OnInteractBtnClick(Button clicker)
    {
        base.OnInteractBtnClick(clicker);
        Inventory.ins.Add(itemData, quantity);
    }
    public void SetQuantity(int quantity)
    {
        this.quantity = quantity;
    }
}
