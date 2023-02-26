using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFunctions : MonoBehaviour
{
    public static GameFunctions ins;
    [SerializeField] private Transform interactBtnContainer;
    [SerializeField] private Canvas mainCanvas;
    private Camera mainCam => Camera.main;
    private static HashSet<string> idOccupation;
    private static CustomRandom randObj;
    private static Color[] markerColors = {Color.blue, Color.red, Color.magenta, Color.yellow};
    private void Awake()
    {
        if (ins == null) ins = this;
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
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(UIManager.ins.currentOpenUI != null) UIManager.ins.ToggleOffCurrentUI();
            else Settings.ins.ToggleSetting();
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
    public static string GenerateId()
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
    
    public static void RevokeId(string id)
    {
        if (idOccupation.Contains(id))
        {
            idOccupation.Remove(id);
        }
    }
    public Vector3 WorldToCanvasPosition(Vector3 worldPos, int projection)
    {
        var camPos = mainCam.transform.worldToLocalMatrix.MultiplyPoint(worldPos).normalized;

        var angleMultiplier = Mathf.Cos(Vector3.Angle(camPos, Vector3.forward) * Mathf.Deg2Rad);
        if (angleMultiplier < 0)
        {
            return -Vector3.one;
        }
        camPos = camPos * (projection / angleMultiplier);
        if (Input.GetKeyDown(KeyCode.E)) Debug.Log(camPos);
        var uiPos = camPos - Vector3.forward * projection;

        var canvasPos = mainCam.ScreenToWorldPoint(new Vector3(0, 0, projection));
        canvasPos = Camera.main.transform.worldToLocalMatrix.MultiplyPoint(canvasPos);
        canvasPos = canvasPos - Vector3.forward * projection;

        uiPos -= canvasPos;

        Vector2 mouseRatio = new Vector2(uiPos.x / (Mathf.Abs(canvasPos.x) * 2), uiPos.y / (Mathf.Abs(canvasPos.y) * 2));
        Vector2 canvasSize = mainCanvas.GetComponent<RectTransform>().position * 2;
        uiPos.z = 0;
        uiPos = new Vector3(canvasSize.x * mouseRatio.x, canvasSize.y * mouseRatio.y);

        return uiPos;
    }
    public static Color GetMarkerColor(int index){
        return markerColors[index];
    }
}
