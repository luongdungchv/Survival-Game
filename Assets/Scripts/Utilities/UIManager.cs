using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    [SerializeField] private GameObject mapUI, inventoryUI, craftUI, anvilUI, furnaceUI;
    [SerializeField] private GameObject interactBtnPrefab, mapCam;
    [SerializeField] private Transform collectBtnContainer;
    [SerializeField] private InventoryInteractionHandler inventoryUIHandler, craftUIHandler, anvilUIHandler, furnaceUIHandler;

    [SerializeField] TransformerUI transformerUI;

    public bool isUIOpen => mapUI.activeSelf ||
                inventoryUI.activeSelf ||
                craftUI.activeSelf ||
                anvilUI.activeSelf ||
                furnaceUI.activeSelf;

    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;

    private Transformer currentOpenCooker;

    private void Awake()
    {
        if (ins == null) ins = this;
    }
    public void ToggleMapUI()
    {
        if (isUIOpen && !mapUI.activeSelf) return;
        mapUI.SetActive(!mapUI.activeSelf);
        mapCam.SetActive(!mapCam.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        //Time.timeScale = mapUI.activeSelf ? 0 : 1;
    }
    public void ToggleInventoryUI()
    {
        if (isUIOpen && !inventoryUI.activeSelf) return;
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen) inventoryUIHandler.SetAsOpen();
        //else inventoryUIHandler.SetAsClose();
        inventoryUIHandler.UpdateUI();
        inventoryUIHandler.ChangeMoveIconQuantity(0);
        //Time.timeScale = inventoryUI.activeSelf ? 0 : 1;
    }
    public void ToggleCraftUI()
    {
        if (isUIOpen && !craftUI.activeSelf) return;
        craftUI.SetActive(!craftUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen) craftUIHandler.SetAsOpen();
        craftUIHandler.UpdateUI();
        craftUIHandler.ChangeMoveIconQuantity(0);
    }
    public void ToggleAnvilUI()
    {
        if (isUIOpen && !anvilUI.activeSelf) return;
        anvilUI.SetActive(!anvilUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen) anvilUIHandler.SetAsOpen();
        anvilUIHandler.UpdateUI();
        anvilUIHandler.ChangeMoveIconQuantity(0);
    }
    public void ToggleFurnaceUI()
    {
        if (isUIOpen && !furnaceUI.activeSelf) return;
        furnaceUI.SetActive(!furnaceUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen)
        {
            furnaceUIHandler.SetAsOpen();
        }
        else
        {
            Transformer.currentOpen = null;
        }
        furnaceUIHandler.UpdateUI();
        furnaceUIHandler.ChangeMoveIconQuantity(0);

    }
    public void ToggleFurnaceUI(Transformer toggler)
    {
        ToggleFurnaceUI();
        Debug.Log(toggler);
        transformerUI.SetTransformer();
    }
    public void RefreshFurnaceUI()
    {
        transformerUI.RefreshUI();
    }
    public void AddCollectBtn(GameObject btn)
    {

        btn.transform.SetParent(collectBtnContainer, false);
    }
    public Button AddCollectBtn(string displayText)
    {
        var btnInstance = Instantiate(interactBtnPrefab);
        btnInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = displayText;
        var btn = btnInstance.GetComponent<Button>();
        //btn.onClick.AddListener(() => OnInteractBtnClick(btn));
        return btn;
    }

}
