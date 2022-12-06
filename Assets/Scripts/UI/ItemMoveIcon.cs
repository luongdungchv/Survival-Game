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
    [SerializeField] private RectTransform baseObj;
    [SerializeField] private int projection;
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
        //Debug.Log(baseObj.position);

    }
    private void FollowMouse()
    {
        var mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, projection));
        mousePosWorld = Camera.main.transform.worldToLocalMatrix.MultiplyPoint(mousePosWorld);
        mousePosWorld = mousePosWorld - Vector3.forward * projection;

        var canvasPosWorld = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, projection));
        canvasPosWorld = Camera.main.transform.worldToLocalMatrix.MultiplyPoint(canvasPosWorld);
        canvasPosWorld = canvasPosWorld - Vector3.forward * projection;

        mousePosWorld = mousePosWorld - canvasPosWorld;
        Vector2 mouseRatio = new Vector2(mousePosWorld.x / (Mathf.Abs(canvasPosWorld.x) * 2), mousePosWorld.y / (Mathf.Abs(canvasPosWorld.y) * 2));
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().position * 2;
        // Vector2 canvasRatio = new Vector2(canvasPos.x * 2, canvasPos.y * 2);
        mousePosWorld.z = 0;
        mousePosWorld = new Vector3(canvasSize.x * mouseRatio.x, canvasSize.y * mouseRatio.y);

        //this.GetComponent<RectTransform>().anchoredPosition = iconPos / (canvas.scaleFactor + 0.1f);


        this.GetComponent<RectTransform>().position = mousePosWorld;
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
