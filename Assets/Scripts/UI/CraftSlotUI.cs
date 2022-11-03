using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string itemName;
    [SerializeField] private int quantity;
    [SerializeField] private RawImage icon;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private Item item => Item.GetItem(itemName);
    private void Awake()
    {
        icon.texture = item.icon;
    }
    private void OnEnable()
    {
        Refresh();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!(item is ICraftable)) return;

        var craftData = (item as ICraftable).requiredMats;
        if (!Refresh()) return;
        foreach (var matName in craftData.Keys)
        {
            var remove = Inventory.ins.Remove(matName, craftData[matName]);
            if (remove)
            {
                iih.movingItem.InitReplaceAction(itemName, quantity);
                //iih.movingItem.movingItem = Item.GetItem(itemName);
            }
        }
        Refresh();
    }
    public bool Refresh()
    {
        var craftData = (item as ICraftable).requiredMats;
        foreach (var mat in craftData)
        {
            if (Inventory.ins.GetItemQuantity(mat.Key) < mat.Value)
            {
                icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0.7f);
                return false;
            }
        }
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1f);
        return true;
    }
}