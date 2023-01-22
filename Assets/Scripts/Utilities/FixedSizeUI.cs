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

        var uiPos = GameFunctions.ins.WorldToCanvasPosition(transform.position, 90);

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
