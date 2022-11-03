using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    [SerializeField] private ToggleData[] datas;
    private int currentTab;
    private void Awake()
    {
        foreach (var i in datas)
        {
            i.toggler.onClick.AddListener(() => Toggle(i.id));
        }
    }
    private void OnEnable()
    {
        Toggle(currentTab);
    }
    private void Toggle(int toggleId)
    {
        foreach (var i in datas)
        {
            if (i.id == toggleId)
            {
                currentTab = i.id;
                i.UI.SetActive(true);
                i.toggler.interactable = false;
            }
            else
            {
                i.toggler.interactable = true;
                i.UI.SetActive(false);
            }
        }

    }


    [Serializable]
    class ToggleData
    {
        public Button toggler;
        public int id;
        public GameObject UI;
    }
}
