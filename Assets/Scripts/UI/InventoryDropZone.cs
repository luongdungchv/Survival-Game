using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IPointerClickHandler
{
    private InventoryInteractionHandler iih => InventoryInteractionHandler.ins;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!iih.isItemMoving) return;
        var dropComplete = Inventory.ins.DropItem(iih.sourceItemIndex, iih.movingCount);
        if (dropComplete)
        {
            iih.ChangeMoveIconQuantity(0);
        }
    }
}