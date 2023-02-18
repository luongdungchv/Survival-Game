using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    [Header("In game UI")]
    [SerializeField] private GameObject mapUI;
    [SerializeField] private GameObject inventoryUI, craftUI, anvilUI, furnaceUI;
    
    [SerializeField] private Transform collectBtnContainer;
    [SerializeField] TransformerUI transformerUI;
    [Header("Notification UI")]
    [SerializeField] private GameObject lostConnectionPanel;
    [SerializeField] private GameObject diePanel, gameOverPanel;
    [SerializeField] private float showDelay;
    [Header("UI Handlers")]
    [SerializeField] private InventoryInteractionHandler inventoryUIHandler;
    private InventoryInteractionHandler craftUIHandler, anvilUIHandler, furnaceUIHandler;
    [Header("Others")]
    [SerializeField] private GameObject interactBtnPrefab;
    private GameObject mapCam;

    

    public bool isUIOpen => mapUI.activeSelf ||
                inventoryUI.activeSelf ||
                craftUI.activeSelf ||
                anvilUI.activeSelf ||
                furnaceUI.activeSelf ||
                lostConnectionPanel.activeSelf ||
                diePanel.activeSelf ||
                gameOverPanel.activeSelf;

    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;

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
        craftUIHandler.CheckAndDropItem();
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
        furnaceUIHandler.CheckAndDropItem();
        furnaceUIHandler.ChangeMoveIconQuantity(0);
        // Debug.Log("click2");
        RefreshFurnaceUI();
    }
    public void ToggleFurnaceUI(Transformer toggler)
    {
        ToggleFurnaceUI();
        RefreshFurnaceUI();
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
    public void ReturnToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void ShowDisconnectPanel()
    {
        //Debug.Log(lostConnectionPanel);
        ThreadManager.ExecuteOnMainThread(() =>
        {
            if(lostConnectionPanel != null) lostConnectionPanel.SetActive(true);
            GameFunctions.ins.ShowCursor();
        });
    }
    public void ShowDiePanel(){
        GameFunctions.ins.ShowCursor();
        IEnumerator ShowDelay(){
            yield return new WaitForSeconds(showDelay);
            diePanel.SetActive(true);
        }
        StartCoroutine(ShowDelay());
    }
    public void ShowGameOverPanel(){
        GameFunctions.ins.ShowCursor();
        IEnumerator ShowDelay(){
            yield return new WaitForSeconds(showDelay);
            gameOverPanel.SetActive(true);
            diePanel.SetActive(false);
        }
        StartCoroutine(ShowDelay());
    }
    public void LeaveGame(){
        Client.ins.LeaveGame();
        SceneManager.LoadScene(0);
        Item.ClearItems();
    }

}
