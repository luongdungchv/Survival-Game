using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    [SerializeField] private GameObject mapUI, inventoryUI;
    [SerializeField] private GameObject interactBtnPrefab, mapCam;
    [SerializeField] private Transform collectBtnContainer;
    public bool isUIOpen => mapUI.activeSelf || inventoryUI.activeSelf;

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
        //Time.timeScale = inventoryUI.activeSelf ? 0 : 1;
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
