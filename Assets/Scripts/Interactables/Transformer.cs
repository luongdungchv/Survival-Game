using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transformer : InteractableObject
{
    public static Transformer currentOpen;
    public InputSlot inputSlot;
    public FuelSlot fuelSlot;
    public OutputSlot outputSlot;
    [SerializeField] private int maxInputCap, maxOutputCap;

    private Coroutine cookRoutine;
    private bool isInCookingState;
    private bool isItemInCook;
    private int currentUnitCount;
    private float currentEfficiency;
    private int cookedUnit;

    // Update is called once per frame
    protected override void OnInteractBtnClick(Button clicker)
    {
        currentOpen = this;
        UIManager.ins.ToggleFurnaceUI(this);
        base.OnInteractBtnClick(clicker);
    }
    public float GetCookProgress()
    {
        if (inputSlot == null) return 0;
        var val = Mathf.InverseLerp(0, inputSlot.inputItem.cookability, cookedUnit);
        return val;
    }
    public float GetRemainingUnit()
    {
        if (fuelSlot == null) return 0;
        var val = Mathf.InverseLerp(0, fuelSlot.fuel.durability, this.currentUnitCount);
        return val;
    }

    public void SetInput(ITransformable inputItem, int quantity)
    {
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
        }

        else if (inputSlot.inputItem.goalItem.itemName != outputSlot.item.itemName)
        {
            if (cookRoutine != null)
            {
                StopCoroutine(cookRoutine);
                isInCookingState = false;
            }
        }
        else if (inputSlot.inputItem.goalItem.itemName == outputSlot.item.itemName)
        {
            cookRoutine = StartCoroutine(CookEnum());
        }

    }
    public void RetrieveInput(int quantity)
    {
        inputSlot.quantity -= quantity;
    }
    public void RetrieveFuel(int quantity)
    {
        fuelSlot.quantity -= quantity;
    }
    public void RetrieveOutput(int quantity)
    {
        outputSlot.quantity -= quantity;
        if (outputSlot.quantity <= 0 && inputSlot.quantity > 0 && !isInCookingState)
        {
            cookRoutine = StartCoroutine(CookEnum());
        }
    }
    public int AddInput(ITransformable additionalItem, int quantity)
    {
        if (inputSlot == null) return 0;
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
    public void SetFuel(IFuel fuel, int quantity)
    {
        fuelSlot = new FuelSlot()
        {
            fuel = fuel,
            quantity = quantity
        };
        currentEfficiency = fuel.efficiency;
        if (!isInCookingState) cookRoutine = StartCoroutine(CookEnum());
    }
    public int AddFuel(IFuel fuel, int quantity)
    {
        if (fuelSlot == null) return 0;
        var currentItem = inputSlot.inputItem as Item;
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
        Debug.Log(inputSlot?.quantity);
        while (true)
        {
            if (inputSlot == null || outputSlot == null || fuelSlot == null) break;
            if (fuelSlot.quantity <= 0 && currentUnitCount == 0 && cookedUnit == 0) break;
            if (outputSlot.quantity == maxOutputCap || (inputSlot.quantity == 0 && !isItemInCook)) break;

            if (currentUnitCount == 0 && fuelSlot.quantity > 0)
            {
                fuelSlot.quantity--;
                currentUnitCount = fuelSlot.fuel.durability;
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
                else
                {
                    currentUnitCount--;
                    cookedUnit++;
                }
            }
            RefreshUI();
            yield return new WaitForSeconds(currentEfficiency);
        }
        isInCookingState = false;
    }
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
    public class FuelSlot
    {
        public IFuel fuel;
        public int quantity;
    }
}
