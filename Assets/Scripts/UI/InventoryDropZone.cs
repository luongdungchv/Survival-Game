using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IPointerClickHandler
{
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!iih.isItemMoving) return;
        var dropComplete = Inventory.ins.DropItem(iih.movingItem.sourceSlot.itemIndex, iih.movingItem.quantity);
        if (dropComplete)
        {
            iih.ChangeMoveIconQuantity(0);
        }
    }
}