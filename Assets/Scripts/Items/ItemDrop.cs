using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ItemDrop : InteractableObject
{
    [SerializeField] private int quantity;
    [SerializeField] private Item itemBase;
    [SerializeField] private bool stackable;

    protected override void OnInteractBtnClick(Button clicker)
    {
        base.OnInteractBtnClick(clicker);
        Inventory.ins.Add(itemBase, quantity);
        Destroy(this.gameObject);
    }
    public void SetQuantity(int quantity)
    {
        this.quantity = quantity;
    }
    public void SetBase(Item itemBase)
    {
        this.itemBase = itemBase;
    }
}
