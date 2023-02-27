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
    [SerializeField] private GameObject inventoryUI, craftUI, anvilUI, furnaceUI, shipRepairUI;

    [SerializeField] private Transform collectBtnContainer;
    [SerializeField] TransformerUI transformerUI;
    [Header("Notification UI")]
    [SerializeField] private GameObject lostConnectionPanel;
    [SerializeField] private GameObject diePanel, gameOverPanel, victoryPanel, commandPanel;
    [SerializeField] private float showDelay;
    [Header("UI Handlers")]
    [SerializeField] private InventoryInteractionHandler inventoryUIHandler;
    [SerializeField] private InventoryInteractionHandler craftUIHandler, anvilUIHandler, furnaceUIHandler, shipRepairUIHandler;
    [Header("Others")]
    [SerializeField] private GameObject interactBtnPrefab;
    [SerializeField] private GameObject mapCam;

    public GameObject currentOpenUI;

    public bool isUIOpen => mapUI.activeSelf ||
                inventoryUI.activeSelf ||
                craftUI.activeSelf ||
                anvilUI.activeSelf ||
                furnaceUI.activeSelf ||
                lostConnectionPanel.activeSelf ||
                diePanel.activeSelf ||
                gameOverPanel.activeSelf ||
                shipRepairUI.activeSelf ||
                commandPanel.activeSelf;

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
        if(mapUI.activeSelf) currentOpenUI = mapUI;
        else if(currentOpenUI == mapUI && !mapUI.activeSelf) currentOpenUI = null;
        GameFunctions.ins.ToggleCursor(isUIOpen);
    }
    public void ToggleInventoryUI()
    {
        if (isUIOpen && !inventoryUI.activeSelf) return;
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen) inventoryUIHandler.SetAsOpen();
        if(inventoryUI.activeSelf) currentOpenUI = inventoryUI;
        else if(currentOpenUI == inventoryUI && !inventoryUI.activeSelf) currentOpenUI = null;
        inventoryUIHandler.UpdateUI();
        inventoryUIHandler.DropMovingItem();
        inventoryUIHandler.ChangeMoveIconQuantity(0);
       
    }
    public void ToggleCraftUI()
    {
        if (isUIOpen && !craftUI.activeSelf) return;
        craftUI.SetActive(!craftUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen) craftUIHandler.SetAsOpen();
        craftUIHandler.UpdateUI();
        craftUIHandler.DropMovingItem();
        if(craftUI.activeSelf) currentOpenUI = craftUI;
        else if(currentOpenUI == craftUI && !craftUI.activeSelf) currentOpenUI = null;
        craftUIHandler.ChangeMoveIconQuantity(0);
    }
    public void ToggleAnvilUI()
    {
        if (isUIOpen && !anvilUI.activeSelf) return;
        anvilUI.SetActive(!anvilUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen) anvilUIHandler.SetAsOpen();
        
        if(anvilUI.activeSelf) currentOpenUI = anvilUI;
        else if(currentOpenUI == anvilUI && !anvilUI.activeSelf) currentOpenUI = null;
        
        anvilUIHandler.UpdateUI();
        anvilUIHandler.DropMovingItem();
        anvilUIHandler.ChangeMoveIconQuantity(0);
    }
    public void ToggleFurnaceUI()
    {
        if (isUIOpen && !furnaceUI.activeSelf) return;
        furnaceUI.SetActive(!furnaceUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (!Client.ins.isHost && !furnaceUI.activeSelf)
        {
            var closePacket = new FurnaceClientMsgPacket()
            {
                playerId = NetworkPlayer.localPlayer.id,
                objId = Transformer.currentOpen.GetComponentInParent<NetworkSceneObject>().id,
                action = "close",
            };
            Client.ins.SendTCPPacket(closePacket);
        }
        TransformerBase.currentOpen.isOpen = furnaceUI.activeSelf;
        if (isUIOpen)
        {
            furnaceUIHandler.SetAsOpen();
        }
        else
        {
            Transformer.currentOpen = null;
        }
        
        if(furnaceUI.activeSelf) currentOpenUI = furnaceUI;
        else if(currentOpenUI == furnaceUI && !furnaceUI.activeSelf) currentOpenUI = null;
        
        furnaceUIHandler.UpdateUI();
        furnaceUIHandler.DropMovingItem();
        furnaceUIHandler.ChangeMoveIconQuantity(0);
        
        RefreshFurnaceUI();
    }
    public void ToggleFurnaceUI(bool state)
    {
        if (isUIOpen && !furnaceUI.activeSelf) return;
        furnaceUI.SetActive(state);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        if (isUIOpen)
        {
            furnaceUIHandler.SetAsOpen();
        }
        else
        {
            Transformer.currentOpen = null;
        }
        
        if(furnaceUI.activeSelf) currentOpenUI = furnaceUI;
        else if(currentOpenUI == furnaceUI && !furnaceUI.activeSelf) currentOpenUI = null;
        
        furnaceUIHandler.UpdateUI();
        furnaceUIHandler.CheckAndDropItem();
        furnaceUIHandler.ChangeMoveIconQuantity(0);
        RefreshFurnaceUI();
    }
    public void ToggleShipRepairUI(){
        if (isUIOpen && !shipRepairUI.activeSelf) return;
        
        shipRepairUI.SetActive(!shipRepairUI.activeSelf);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        
        if(!shipRepairUI.activeSelf && !Client.ins.isHost){
            var closePacket = new RawActionPacket(PacketType.ShipInteraction){
                playerId = NetworkPlayer.localPlayer.id,
                objId = "0",
                action = "close"  
            };
            Client.ins.SendTCPPacket(closePacket);
        }
        
        if(shipRepairUI.activeSelf) currentOpenUI = shipRepairUI;
        else if(currentOpenUI == shipRepairUI && !shipRepairUI.activeSelf) currentOpenUI = null;
        
        shipRepairUIHandler.UpdateUI();
        shipRepairUIHandler.DropMovingItem();
        shipRepairUIHandler.ChangeMoveIconQuantity(0);
    }
    public void ToggleShipRepairUI(bool state){
        if (isUIOpen && !shipRepairUI.activeSelf) return;
        shipRepairUI.SetActive(state);
        GameFunctions.ins.ToggleCursor(isUIOpen);
        
        if(shipRepairUI.activeSelf) currentOpenUI = shipRepairUI;
        else if(currentOpenUI == shipRepairUI && !shipRepairUI.activeSelf) currentOpenUI = null;
        
        shipRepairUIHandler.UpdateUI();
        shipRepairUIHandler.DropMovingItem();
        shipRepairUIHandler.ChangeMoveIconQuantity(0);
    }
    public void ToggleOffCurrentUI(){
        if(currentOpenUI == mapUI) ToggleMapUI();
        if(currentOpenUI == inventoryUI) ToggleInventoryUI();
        if(currentOpenUI == craftUI) ToggleCraftUI();
        if(currentOpenUI == furnaceUI) ToggleFurnaceUI();
        if(currentOpenUI == anvilUI) ToggleAnvilUI();
        if(currentOpenUI == shipRepairUI) ToggleShipRepairUI();
        if(currentOpenUI == commandPanel) ToggleCommandUI();
    }
    public void ToggleCommandUI(){
        this.commandPanel.SetActive(!commandPanel.activeSelf);
        
        if(this.commandPanel.activeSelf) currentOpenUI = this.commandPanel;
        else if(currentOpenUI == this.commandPanel && !this.commandPanel.activeSelf) 
            currentOpenUI = null;  
        DeveloperPanel.instance.FocusInputField();
        GameFunctions.ins.ToggleCursor(isUIOpen);
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
        return btn;
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void ShowDisconnectPanel()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            if (lostConnectionPanel != null) lostConnectionPanel.SetActive(true);
            GameFunctions.ins.ShowCursor();
        });
    }
    public void ShowDiePanel()
    {
        GameFunctions.ins.ShowCursor();
        IEnumerator ShowDelay()
        {
            yield return new WaitForSeconds(showDelay);
            diePanel.SetActive(true);
        }
        StartCoroutine(ShowDelay());
    }
    public void ShowGameOverPanel()
    {
        GameFunctions.ins.ShowCursor();
        IEnumerator ShowDelay()
        {
            yield return new WaitForSeconds(showDelay);
            gameOverPanel.SetActive(true);
            diePanel.SetActive(false);
        }
        StartCoroutine(ShowDelay());
    }
    public void ShowVictoryPanel(){
        GameFunctions.ins.ShowCursor();
        IEnumerator ShowDelay()
        {
            yield return new WaitForSeconds(showDelay);
            victoryPanel.SetActive(true);
            diePanel.SetActive(false);
        }
        StartCoroutine(ShowDelay());
    }
    public void LeaveGame()
    {
        Client.ins.LeaveGame();
        SceneManager.LoadScene(0);
    }

}
