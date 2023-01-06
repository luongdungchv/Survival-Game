//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFunctions : MonoBehaviour
{
    public static GameFunctions ins;
    [SerializeField] private Transform interactBtnContainer;
    private HashSet<string> idOccupation;
    private CustomRandom randObj;
    private void Awake()
    {
        if (ins == null) ins = this;

    }
    private void Start()
    {
        //HideCursor();
        idOccupation = new HashSet<string>();
        randObj = new CustomRandom(MapGenerator.ins.seed);

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
    public string GenerateId()
    {
        char[] temp = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };

        while (idOccupation.Contains(new string(temp)))
        {
            HashSet<int> charOccupation = new HashSet<int>();
            temp = new char[6];
            int i = 0;
            while (i < 6)
            {
                int rand = randObj.Next(100, 122);
                while (charOccupation.Contains(rand))
                {
                    rand = randObj.Next(100, 122);
                }
                temp[i] = (char)rand;
                i++;
            }
        }
        var res = new string(temp);
        idOccupation.Add(res);
        return res;
    }
    public void RevokeId(string id)
    {
        if (idOccupation.Contains(id))
        {
            idOccupation.Remove(id);
        }
    }

}
