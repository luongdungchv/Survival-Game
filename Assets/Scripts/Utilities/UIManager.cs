using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    [SerializeField] private GameObject mapObj, collectBtnPrefab, mapCam;
    [SerializeField] private Transform collectBtnContainer;
    public bool isMapOpen => mapObj.activeSelf;

    private void Start()
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

}
