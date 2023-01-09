using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformerClient : TransformerBase
{
    // TODO: Handle on interact btn click
    protected override void Awake()
    {
        if (Client.ins.isHost)
        {
            Destroy(this);
            return;
        }
        base.Awake();
    }
    protected override void OnInteractBtnClick(Button clicker)
    {
        Debug.Log("click");
        currentOpen = this;
        UIManager.ins.ToggleFurnaceUI();
        base.OnInteractBtnClick(clicker);
    }
    public override void SetInput(ITransformable inputItem, int quantity)
    {
        var setInputPacket = new FurnaceClientMsgPacket()
        {
            playerId = Client.ins.clientId,
            objId = GetComponentInParent<NetworkSceneObject>().id,
            action = "set_input",
            actionParams = new string[] { (inputItem as Item).itemName, quantity.ToString() }
        };
        Debug.Log($"set_input pack: {setInputPacket.GetString()}");
        Client.ins.SendTCPPacket(setInputPacket);

    }
    public void SetInput(int quantity)
    {
        SetInput(inputSlot.inputItem, quantity);
    }
    public void ReceiveInput(ITransformable inputItem, int quantity)
    {
        inputSlot.inputItem = inputItem;
        inputSlot.quantity = quantity;
    }
    public void ReceiveInput(int quantity)
    {
        if (inputSlot.inputItem == null) return;
        inputSlot.quantity = quantity;
    }
    public override void SetFuel(IFuel fuel, int quantity)
    {
        var retrOutputPacket = new FurnaceClientMsgPacket()
        {
            playerId = Client.ins.clientId,
            objId = GetComponentInParent<NetworkSceneObject>().id,
            action = "set_fuel",
            actionParams = new string[] { (fuel as Item).itemName, quantity.ToString() }
        };
        Client.ins.SendTCPPacket(retrOutputPacket);
    }
    public void SetFuel(int quantity)
    {
        SetFuel(fuelSlot.fuel, quantity);
    }
    public void ReceiveFuel(IFuel fuelItem, int quantity)
    {
        fuelSlot.fuel = fuelItem;
        fuelSlot.quantity = quantity;
    }
    public void ReceiveFuel(int quantity)
    {
        if (fuelSlot.fuel == null) return;
        fuelSlot.quantity = quantity;
    }
    public override void RetrieveInput(int quantity)
    {
        var retrInputPacket = new FurnaceClientMsgPacket()
        {
            playerId = Client.ins.clientId,
            objId = GetComponentInParent<NetworkSceneObject>().id,
            action = "retr_input",
            actionParams = new string[] { quantity.ToString() }
        };
        Client.ins.SendTCPPacket(retrInputPacket);
        //TODO: Handle retrieving input on client
        inputSlot.quantity -= quantity;
    }
    public override void RetrieveFuel(int quantity)
    {
        var retrFuelPacket = new FurnaceClientMsgPacket()
        {
            playerId = Client.ins.clientId,
            objId = GetComponentInParent<NetworkSceneObject>().id,
            action = "retr_fuel",
            actionParams = new string[] { quantity.ToString() }
        };
        Client.ins.SendTCPPacket(retrFuelPacket);
        fuelSlot.quantity -= quantity;
        //TODO: Handle retrieving fuel on client
    }
    public void ReceiveOutput(Item outputItem, int quantity)
    {
        outputSlot.item = outputItem;
        outputSlot.quantity = quantity;
    }
    public void ReceiveOutput(int quantity)
    {
        if (outputSlot.item == null) return;
        outputSlot.quantity = quantity;
    }
    public void ReceiveProgressInfo(int cookedUnit, int remainingUnit)
    {
        this.cookedUnit = cookedUnit;
        this.remainingUnit = remainingUnit;
    }
    public override void RetrieveOutput(int quantity)
    {
        var retrOutputPacket = new FurnaceClientMsgPacket()
        {
            playerId = Client.ins.clientId,
            objId = GetComponentInParent<NetworkSceneObject>().id,
            action = "retr_output",
            actionParams = new string[] { quantity.ToString() }
        };
        Client.ins.SendTCPPacket(retrOutputPacket);
        outputSlot.quantity -= quantity;
        //TODO: Handle retrieving output on client
    }

}
