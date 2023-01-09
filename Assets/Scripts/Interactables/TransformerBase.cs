using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformerBase : InteractableObject
{
    // TODO: test singleton
    public static TransformerBase currentOpen;
    public InputSlot inputSlot;
    public FuelSlot fuelSlot;
    public OutputSlot outputSlot;
    [SerializeField] protected int maxInputCap, maxOutputCap;

    protected bool isInCookingState;
    protected bool isItemInCook;
    protected int remainingUnit;
    protected float currentEfficiency;
    protected int cookedUnit;
    // protected override void OnInteractBtnClick(Button clicker)
    // {
    //     currentOpen = this;
    //     UIManager.ins.ToggleFurnaceUI();
    //     base.OnInteractBtnClick(clicker);
    // }
    public float GetCookProgress()
    {
        if (inputSlot == null || inputSlot.inputItem == null) return 0;
        var val = Mathf.InverseLerp(0, inputSlot.inputItem.cookability, cookedUnit);
        return val;
    }
    public float GetRemainingUnit()
    {
        if (fuelSlot == null || fuelSlot.fuel == null) return 0;
        var val = Mathf.InverseLerp(0, fuelSlot.fuel.durability, this.remainingUnit);
        return val;
    }

    public virtual void SetInput(ITransformable inputItem, int quantity)
    {


    }
    public int AddInput(int quantity)
    {
        if (inputSlot == null) return 0;
        var currentItem = inputSlot.inputItem;
        return AddInput(currentItem, quantity);
    }
    public virtual void RetrieveInput(int quantity)
    {

    }
    public virtual void RetrieveFuel(int quantity)
    {

    }
    public virtual void RetrieveOutput(int quantity)
    {

    }
    public virtual int AddInput(ITransformable additionalItem, int quantity)
    {
        return 0;
    }
    private void RefreshUI()
    {
        UIManager.ins.RefreshFurnaceUI();
    }
    public virtual void SetFuel(IFuel fuel, int quantity)
    {


    }
    public virtual int AddFuel(IFuel fuel, int quantity)
    {
        return 0;
    }
    public int AddFuel(int quantity)
    {
        if (inputSlot == null) return 0;
        var currentItem = fuelSlot.fuel;
        return AddFuel(currentItem, quantity);
    }
}
[System.Serializable]
public class InputSlot
{
    public ITransformable inputItem;
    public int quantity;
}
[System.Serializable]
public class OutputSlot
{
    public Item item;
    public int quantity;
}
[System.Serializable]
public class FuelSlot
{
    public IFuel fuel;
    public int quantity;
}
