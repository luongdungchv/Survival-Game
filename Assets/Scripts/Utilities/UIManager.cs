using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    [SerializeField] private GameObject mapObj;

    private void Start()
    {
        if (ins == null) ins = this;
    }
    public void ToggleMap()
    {
        mapObj.SetActive(!mapObj.activeSelf);
        Time.timeScale = mapObj.activeSelf ? 0 : 1;
    }

}
