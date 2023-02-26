﻿using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class Transformer : TransformerBase
{
    [SerializeField] private float syncRate;
    float elapsed = 0;
    private Coroutine cookRoutine;
    private FurnaceUpdatePacket updatePacket = new FurnaceUpdatePacket();
    
    protected override void Awake()
    {
        if (!Client.ins.isHost)
        {
            Destroy(this);
            return;
        }
        base.Awake();

    }
    private IEnumerator Start()
    {
        base.Start();
        yield return null;
        updatePacket.playerId = Client.ins.clientId;
        updatePacket.objId = GetComponentInParent<NetworkSceneObject>().id;
    }
    protected override void OnInteractBtnClick(Button clicker)
    {
        currentOpen = this;
        if(!isOpen){
            isOpen = true;
            UIManager.ins.ToggleFurnaceUI();
        }
        base.OnInteractBtnClick(clicker);
    }
    protected override void Update()
    {
        base.Update();
        if (Client.ins.isHost)
        {
            if (elapsed >= syncRate)
            {
                updatePacket.inputCount = inputSlot.quantity;
                updatePacket.fuelCount = fuelSlot.quantity;
                updatePacket.outputCount = outputSlot.quantity;

                var inputItemName = inputSlot.inputItem == null ? "" : (inputSlot.inputItem as Item).itemName;
                var fuelItemName = fuelSlot.fuel == null ? "" : (fuelSlot.fuel as Item).itemName;
                var outputItemName = outputSlot.item == null ? "" : outputSlot.item.itemName;

                updatePacket.inputItem = inputItemName == updatePacket.inputItem
                            ? "" : inputItemName;

                updatePacket.fuelItem = fuelItemName == updatePacket.fuelItem
                ? "" : fuelItemName;

                updatePacket.outputItem = outputItemName == updatePacket.outputItem
                ? "" : outputItemName;

                updatePacket.cookedUnit = cookedUnit;
                updatePacket.remainingUnit = remainingUnit;

                Client.ins.SendUDPPacket(updatePacket);
                elapsed = 0;
            }
            elapsed += Time.deltaTime;
        }

    }
    

    public override bool SetInput(ITransformable inputItem, int quantity)
    {
        if(cookedUnit > 0 && inputItem != inputSlot.inputItem) return false;
        var redundant = quantity - maxInputCap;

        inputSlot = new InputSlot()
        {
            inputItem = inputItem,
            quantity = redundant <= 0 ? quantity : maxInputCap
        };
        if (outputSlot == null || outputSlot.quantity == 0)
        {
            outputSlot = new OutputSlot()
            {
                item = inputItem.goalItem,
                quantity = 0
            };
            cookRoutine = StartCoroutine(CookEnum());
            return true;
        }

        else if (inputSlot.inputItem.goalItem.itemName != outputSlot.item.itemName)
        {
            if (cookRoutine != null)
            {
                StopCoroutine(cookRoutine);
                isInCookingState = false;
                return true;
            }
        }
        else if (inputSlot.inputItem.goalItem.itemName == outputSlot.item.itemName)
        {
            cookRoutine = StartCoroutine(CookEnum());
            return true;
        }
        return false;
    }
    public override void RetrieveInput(int quantity)
    {
        if(quantity == -1) inputSlot.quantity = 0;
        else inputSlot.quantity -= quantity; 
    }
    public override void RetrieveFuel(int quantity)
    {
        fuelSlot.quantity -= quantity;
    }
    public override void RetrieveOutput(int quantity)
    {
        outputSlot.quantity -= quantity;
        if (outputSlot.quantity <= 0 && inputSlot.quantity > 0 && !isInCookingState)
        {
            if (outputSlot.quantity == 0 && inputSlot.inputItem != null && inputSlot.quantity != 0)
                outputSlot = new OutputSlot()
                {
                    item = inputSlot.inputItem.goalItem,
                    quantity = 0
                };
            cookRoutine = StartCoroutine(CookEnum());
        }
    }
    public override int AddInput(ITransformable additionalItem, int quantity)
    {
        if (inputSlot == null || inputSlot.inputItem == null) return 0;
        var currentItem = inputSlot.inputItem as Item;
        var addItem = additionalItem as Item;
        int redundant = 0;
        if (currentItem.itemName == addItem.itemName)
        {
            inputSlot.quantity += quantity;
            if (inputSlot.quantity > maxInputCap)
            {
                redundant = quantity - maxInputCap;
                inputSlot.quantity = maxInputCap;
            }
        }

        return redundant;
    }
    private void RefreshUI()
    {

        UIManager.ins.RefreshFurnaceUI();

    }
    public override void SetFuel(IFuel fuel, int quantity)
    {
        fuelSlot = new FuelSlot()
        {
            fuel = fuel,
            quantity = quantity
        };
        currentEfficiency = fuel.efficiency;
        if (!isInCookingState) cookRoutine = StartCoroutine(CookEnum());
    }
    public override int AddFuel(IFuel fuel, int quantity)
    {
        if (fuelSlot == null || fuelSlot.fuel == null) return 0;
        var currentItem = fuelSlot.fuel as Item;
        var addItem = fuel as Item;
        var redundant = 0;
        if (currentItem.itemName == addItem.itemName)
        {
            fuelSlot.quantity += quantity;
            if (fuelSlot.quantity > maxInputCap)
            {
                redundant = quantity - maxInputCap;
                fuelSlot.quantity = maxInputCap;
            }
        }
        return redundant;
    }
    IEnumerator CookEnum()
    {

        isInCookingState = true;
        while (true)
        {
            if (inputSlot == null || outputSlot == null || fuelSlot == null) break;
            if (inputSlot.inputItem == null || outputSlot.item == null || fuelSlot.fuel == null) break;
            if (fuelSlot.quantity <= 0 && remainingUnit == 0 && cookedUnit == 0) break;
            if (outputSlot.item != null && outputSlot.item != (inputSlot.inputItem.goalItem)) break;
            if (outputSlot.quantity == maxOutputCap || (inputSlot.quantity == 0 && !isItemInCook)) break;

            if (remainingUnit == 0 && fuelSlot.quantity > 0)
            {
                fuelSlot.quantity--;
                remainingUnit = fuelSlot.fuel.durability;
            }
            if (!isItemInCook)
            {
                inputSlot.quantity--;
                isItemInCook = true;
            }
            else
            {
                if (cookedUnit == inputSlot.inputItem.cookability)
                {
                    cookedUnit = 0;
                    outputSlot.quantity++;
                    isItemInCook = false;
                }
                else if (remainingUnit > 0)
                {
                    remainingUnit--;
                    cookedUnit++;
                }
            }
            RefreshUI();
            yield return new WaitForSeconds(currentEfficiency);
        }
        isInCookingState = false;
    }
}
