using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformerUI : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private CookInputSlotUI inputSlot;
    [SerializeField] private FuelSlotUI fuelSlot;
    [SerializeField] private CookOutputSlotUI outputSlot;
    [SerializeField] private Slider cookProgressBar, remainingUnitBar;
    private void Update()
    {
        cookProgressBar.value = Transformer.currentOpen.GetCookProgress();
        remainingUnitBar.value = Transformer.currentOpen.GetRemainingUnit();
    }
    public void SetTransformer()
    {
    }
    public void RefreshUI()
    {
        if (Transformer.currentOpen == null) return;
        inputSlot.CheckIconVisibility();
        fuelSlot.CheckIconVisibility();
        outputSlot.CheckIconVisibility();
    }
}
