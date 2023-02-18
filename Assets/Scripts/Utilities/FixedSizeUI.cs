using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedSizeUI : MonoBehaviour
{
    [SerializeField] private Image uiElement;
    [SerializeField] private float projection;
    [SerializeField] public Transform uiWorldPos;
    [SerializeField] private float distanceThreshold = 20;
    private PlayerNearbyDetector playerDetector;
    public Canvas canvas;
    private Camera mainCam => Camera.main;
    private bool isDisplayed;
    private Transform playerTransform;
    private void Start()
    {
        // playerDetector = GetComponent<PlayerNearbyDetector>();
        // if (playerDetector == null)
        // {
        //     playerDetector = GetComponentInChildren<PlayerNearbyDetector>();
        // }
        // if (playerDetector == null)
        // {
        //     playerDetector = GetComponentInParent<PlayerNearbyDetector>();
        // }
        // if (uiWorldPos == null) uiWorldPos = this.transform;
        // playerTransform = NetworkPlayer.localPlayer.transform;
        // ScriptCullingManager.ins.AddToCullingList(this.gameObject, distanceThreshold);
        // Destroy(this);
    }
    // Start is called before the first frame update
    public void UpdateMethod()
    {
        // if(Vector3.Distance(transform.position, playerTransform.position) > distanceThreshold) return;
        // playerDetector.DetectHit();
        // if (!isDisplayed) return;

        // var uiPos = GameFunctions.ins.WorldToCanvasPosition(uiWorldPos.position, 90);

        // uiElement.GetComponent<RectTransform>().position = uiPos;
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
