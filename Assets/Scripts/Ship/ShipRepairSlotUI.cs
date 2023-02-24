using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShipRepairSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Item targetItem;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private int requiredAmount;
    private int _quantity;
    public int quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            quantityText.text = value.ToString();
            if (value >= requiredAmount) quantityText.color = Color.green;
            else quantityText.color = Color.red;
        }
    }
    private ShipRepair repairManager => ShipRepair.ins;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private void OnEnable()
    {
        this.UpdateUI();
    }
    private void Start()
    {
        ShipRepair.ins.RegisterOnItemSet(this.UpdateUI);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (iih.isItemMoving)
        {
            if (iih.movingItemHolder.movingItem != targetItem) return;
            if (Inventory.ins.Remove(iih.movingItemHolder.movingItem.itemName, iih.movingItemHolder.quantity))
            {
                //quantity += iih.movingItemHolder.quantity;
                if (Client.ins.isHost)
                {
                    repairManager.SetItem(targetItem, quantity + iih.movingItemHolder.quantity);
                }
                var shipRepairPacket = new RawActionPacket(PacketType.ShipInteraction)
                {
                    playerId = NetworkPlayer.localPlayer.id,
                    objId = "0",
                    action = "set_item",
                    actionParams = new string[] { targetItem.itemName, (quantity + iih.movingItemHolder.quantity).ToString() }
                };
                Client.ins.SendTCPPacket(shipRepairPacket);
                this.UpdateUI();
                iih.ChangeMoveIconQuantity(0);
            }
        }
    }
    private void UpdateUI()
    {
        this.quantity = this.repairManager.GetCurrentAmount(targetItem);
    }
}
