using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedSizeUI : MonoBehaviour
{
    [SerializeField] private Image uiElement;
    [SerializeField] private float projection;
    [SerializeField] public Transform uiWorldPos;
    private PlayerNearbyDetector playerDetector;
    public Canvas canvas;
    private Camera mainCam => Camera.main;
    private bool isDisplayed;
    private void Start()
    {
        playerDetector = GetComponent<PlayerNearbyDetector>();
        if (playerDetector == null)
        {
            playerDetector = GetComponentInChildren<PlayerNearbyDetector>();
        }
        if (playerDetector == null)
        {
            playerDetector = GetComponentInParent<PlayerNearbyDetector>();
        }
        if (uiWorldPos == null) uiWorldPos = this.transform;
    }
    // Start is called before the first frame update
    private void Update()
    {
        playerDetector.DetectHit();
        if (!isDisplayed) return;

        var uiPos = GameFunctions.ins.WorldToCanvasPosition(uiWorldPos.position, 90);

        uiElement.GetComponent<RectTransform>().position = uiPos;
    }
    public void SetDisplay(bool display)
    {
        try
        {
            uiElement?.gameObject.SetActive(display);
            this.isDisplayed = display;
        }
        catch { }
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
