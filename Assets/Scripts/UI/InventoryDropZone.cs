using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IPointerClickHandler
{
    private InventoryInteractionHandler iih => InventoryInteractionHandler.ins;
    [SerializeField] private Transform dropPos;
    public void OnPointerClick(PointerEventData eventData)
    {
        var dropComplete = Inventory.ins.DropItem(iih.sourceItemIndex, iih.movingCount);
        if (dropComplete)
        {
            iih.ChangeMoveIconQuantity(0);
        }
    }
}