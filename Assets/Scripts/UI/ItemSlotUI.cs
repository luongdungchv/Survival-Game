using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{

    public int itemSlotIndex, quantity;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;

    public void OnPointerClick(PointerEventData eventData)
    {
    }

}

