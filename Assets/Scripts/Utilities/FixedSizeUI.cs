using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedSizeUI : MonoBehaviour
{
    [SerializeField] private Image uiElement;
    [SerializeField] private float projection;
    private PlayerNearbyDetector playerDetector;
    public Canvas canvas;
    private Camera mainCam => Camera.main;
    private bool isDisplayed;
    private void Start()
    {
        playerDetector = GetComponent<PlayerNearbyDetector>();
    }
    // Start is called before the first frame update
    private void Update()
    {
        playerDetector.DetectHit();
        if (!isDisplayed) return;
        var camPos = mainCam.transform.worldToLocalMatrix.MultiplyPoint(transform.position).normalized;

        camPos = camPos * (projection / Mathf.Cos(Vector3.Angle(camPos, Vector3.forward) * Mathf.Deg2Rad));
        var uiPos = camPos - Vector3.forward * projection;

        var canvasPos = mainCam.ScreenToWorldPoint(new Vector3(0, 0, projection));
        canvasPos = Camera.main.transform.worldToLocalMatrix.MultiplyPoint(canvasPos);
        canvasPos = canvasPos - Vector3.forward * projection;

        uiPos -= canvasPos;

        Vector2 mouseRatio = new Vector2(uiPos.x / (Mathf.Abs(canvasPos.x) * 2), uiPos.y / (Mathf.Abs(canvasPos.y) * 2));
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().position * 2;
        // Vector2 canvasRatio = new Vector2(canvasPos.x * 2, canvasPos.y * 2);
        uiPos.z = 0;
        uiPos = new Vector3(canvasSize.x * mouseRatio.x, canvasSize.y * mouseRatio.y);

        uiElement.GetComponent<RectTransform>().position = uiPos;
    }
    public void SetDisplay(bool display)
    {
        uiElement.gameObject.SetActive(display);
        this.isDisplayed = display;
    }
    public void SetUIElement(Image ui)
    {
        this.uiElement = ui;
    }
    public void SetElementValue(float val)
    {
        uiElement.fillAmount = val;
    }
}
