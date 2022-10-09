using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFunctions : MonoBehaviour
{
    [SerializeField] private Transform interactBtnContainer;
    private void Start()
    {
        HideCursor();
    }
    private void Update()
    {
        if (InputReader.ins.ShowCursorKeyPress())
        {
            ShowCursor();
        }
        if (InputReader.ins.ShowCursorKeyRelease())
        {
            HideCursor();
        }
        if (InputReader.ins.InteractBtnPress())
        {
            if (interactBtnContainer.childCount > 0)
            {
                var btn = interactBtnContainer.GetChild(0).GetComponent<Button>();
                btn.onClick.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            UIManager.ins.ToggleMap();
        }
    }
    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("hide");
    }
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("show");
    }

}
