using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    [SerializeField] private GameObject mapObj, interactBtnPrefab, mapCam;
    [SerializeField] private Transform collectBtnContainer;
    public bool isMapOpen => mapObj.activeSelf;

    private void Awake()
    {
        if (ins == null) ins = this;
    }
    public void ToggleMap()
    {
        mapObj.SetActive(!mapObj.activeSelf);
        mapCam.SetActive(!mapCam.activeSelf);
        Time.timeScale = mapObj.activeSelf ? 0 : 1;
    }
    public void AddCollectBtn(GameObject btn)
    {
        Debug.Log("enter");

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
