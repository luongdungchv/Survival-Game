using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShipRepair : InteractableObject
{
    public static ShipRepair ins;
    [SerializeField] private GameObject shipRepairUI;
    [SerializeField] private InventoryInteractionHandler iih;
    [SerializeField] private List<RepairDetail> repairDetailList;
    private UnityEvent OnItemSet;
    public bool isOpen;

    private Dictionary<Item, int> repairDetails;
    private Dictionary<Item, int> currentDetails;

    protected override void Awake()
    {
        base.Awake();
        ins = this;

        repairDetails = new Dictionary<Item, int>();
        currentDetails = new Dictionary<Item, int>();
        
        OnItemSet = new UnityEvent();

        foreach (var i in repairDetailList)
        {
            repairDetails.Add(i.item, i.quantity);
            currentDetails.Add(i.item, 0);
        }
    }
    protected override void OnInteractBtnClick(Button clicker)
    {
        base.OnInteractBtnClick(clicker);
        if (Client.ins.isHost) UIManager.ins.ToggleShipRepairUI();
        var shipRepairPacket = new RawActionPacket(PacketType.ShipInteraction)
        {
            playerId = NetworkPlayer.localPlayer.id,
            objId = "0",
            action = "open",
        };
        Client.ins.SendTCPPacket(shipRepairPacket);
    }

    // Update is called once per frame
    public bool AddItem(Item repairItem, int quantity)
    {
        if (!currentDetails.ContainsKey(repairItem)) return false;
        currentDetails[repairItem] += quantity;
        CheckVictory();
        return true;
    }
    public bool SetItem(Item repairItem, int quantity)
    {
        if (!currentDetails.ContainsKey(repairItem)) return false;
        currentDetails[repairItem] = quantity;
        OnItemSet.Invoke();
        CheckVictory();
        return true;
    }
    public void RegisterOnItemSet(UnityAction callback){
        OnItemSet.AddListener(callback);
    }
    public int GetRequiredAmount(Item item)
    {
        return repairDetails[item];
    }
    public int GetCurrentAmount(Item item)
    {
        return currentDetails[item];
    }
    private void CheckVictory()
    {
        foreach (var i in repairDetails.Keys)
        {
            if (repairDetails[i] > currentDetails[i])
            {
                Debug.Log("Not victory yet");
                return;
            }
        }
        Debug.Log("Victory");
    }
    [System.Serializable]
    public class RepairDetail
    {
        public Item item;
        public int quantity;
    }
}
