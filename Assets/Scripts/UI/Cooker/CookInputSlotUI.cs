using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CookInputSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RawImage icon;
    [SerializeField] private TextMeshProUGUI quantityText;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private Transformer currentTransformer => Transformer.currentOpen;
    private int quantity => currentTransformer.inputSlot.quantity;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (iih.isItemMoving)
        {
            if (!(iih.movingItem.movingItem is ITransformable)) return;
            var moving = iih.movingItem;
            var movingName = moving.movingItem.itemName;

            if (currentTransformer.inputSlot == null || currentTransformer.inputSlot.quantity == 0)
            {
                if (!(moving.movingItem is ITransformable)) return;
                currentTransformer.SetInput(moving.movingItem as ITransformable, moving.quantity);
                Inventory.ins.Remove(movingName, moving.quantity);
                moving.quantity = 0;
                CheckIconVisibility();
                icon.texture = moving.icon;
            }

            var inputItem = currentTransformer.inputSlot.inputItem;
            if (movingName == (inputItem as Item).name)
            {
                var odd = currentTransformer.AddInput(inputItem, moving.quantity);
                Inventory.ins.Remove(moving.movingItem.itemName, moving.quantity - odd);
                moving.InitReplaceAction(movingName, odd);
                CheckIconVisibility();
            }
        }
        else
        {
            var inputItem = currentTransformer.inputSlot.inputItem as Item;
            var inputQuantity = currentTransformer.inputSlot.quantity;
            iih.movingItem.InitReplaceAction(inputItem.itemName, inputQuantity);
            currentTransformer.RetrieveInput(inputQuantity);
            CheckIconVisibility();
        }
    }
    public void CheckIconVisibility()
    {
        if (currentTransformer == null || currentTransformer.inputSlot == null)
        {
            icon.gameObject.SetActive(false);
            return;
        };
        if (quantity <= 0) icon.gameObject.SetActive(false);
        else
        {
            icon.gameObject.SetActive(true);
            quantityText.text = quantity.ToString();
        }
    }
    // public void SetTransformer(Transformer a)
    // {
    //     this.currentTransformer = a;
    // }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
