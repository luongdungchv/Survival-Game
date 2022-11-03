using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{

    public int itemSlotIndex, quantity;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;

    public void OnPointerClick(PointerEventData eventData)
    {
        //iih.SetMovingItem(this);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

