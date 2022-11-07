using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFunctions : MonoBehaviour
{
    public static GameFunctions ins;
    [SerializeField] private Transform interactBtnContainer;
    private void Awake()
    {
        if (ins == null) ins = this;
    }
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
        if (InputReader.ins.OpenInventoryPress())
        {
            UIManager.ins.ToggleInventoryUI();
        }
        if (InputReader.ins.OpenMapPress())
        {
            UIManager.ins.ToggleMapUI();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            UIManager.ins.ToggleCraftUI();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            UIManager.ins.ToggleAnvilUI();
        }
    }
    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ToggleCursor(bool show)
    {
        if (show) ShowCursor();
        else HideCursor();
    }

}
