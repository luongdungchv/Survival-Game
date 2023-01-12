using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IPointerClickHandler
{
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!iih.isItemMoving) return;
        if (iih.movingItem.sourceSlot != null)
        {

            var dropPos = NetworkPlayer.localPlayer.transform.position + Vector3.up * 2;
            var dropComplete = Inventory.ins.DropItem(iih.movingItem.sourceSlot.itemIndex, iih.movingItem.quantity, dropPos);
            if (dropComplete)
            {
                iih.ChangeMoveIconQuantity(0);
            }
        }
        else
        {
            iih.CheckAndDropItem();
            iih.ChangeMoveIconQuantity(0);
        }
    }
}