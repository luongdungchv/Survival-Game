using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemMoveIcon : MonoBehaviour
{
    [SerializeField] RawImage iconImage;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] private Camera mainCam;
    public Item movingItem;
    public int sourceIndex => sourceSlot.itemIndex;
    public InventorySlotUI sourceSlot;
    public Canvas canvas;
    public Texture2D _icon;
    public Texture2D icon
    {
        get => _icon;
        set
        {
            _icon = value;
            iconImage.texture = _icon;
        }
    }
    public int _quantity;
    public int quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            quantityText.text = _quantity.ToString();
            if (value > 0) iconImage.gameObject.SetActive(true);
            else iconImage.gameObject.SetActive(false);
        }
    }
    private Inventory inventory => Inventory.ins;
    private void OnEnable()
    {
        FollowMouse();
    }
    private void Update()
    {
        FollowMouse();
    }
    private void FollowMouse()
    {
        var mousePosWorld = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCam.farClipPlane));

        mousePosWorld = mainCam.transform.worldToLocalMatrix.MultiplyPoint(mousePosWorld);


        mousePosWorld = mousePosWorld - Vector3.forward * mainCam.farClipPlane;
        Vector2 iconPos = new Vector2(mousePosWorld.x, mousePosWorld.y);

        this.GetComponent<RectTransform>().anchoredPosition = iconPos / (canvas.scaleFactor + 0.1f);

        if (gameObject.activeSelf && Input.mouseScrollDelta.y != 0 && sourceSlot != null)
        {
            quantity += (int)Input.mouseScrollDelta.y;
            sourceSlot.quantity -= (int)Input.mouseScrollDelta.y;

            int baseItemQuantity = inventory.items[sourceSlot.itemIndex].quantity;
            sourceSlot.quantity = Mathf.Clamp(sourceSlot.quantity, 0, Mathf.Min(baseItemQuantity, inventory.maxInventorySlot) - 1);
            quantity = Mathf.Clamp(quantity, 1, Mathf.Min(baseItemQuantity, inventory.maxInventorySlot));
        }
    }

    public void InitMoveAction(InventorySlotUI source)
    {
        quantity = source.quantity;
        source.quantity = 0;
        icon = source.icon;
        //sourceIndex = source.itemIndex;
        movingItem = source.item;
        sourceSlot = source;
    }
    public void InitReplaceAction(string itemName, int quantity)
    {
        var itemData = Item.GetItem(itemName);
        Debug.Log(itemName);
        movingItem = itemData;
        this.quantity = quantity;
        sourceSlot = null;
        icon = itemData.icon;
    }
}
