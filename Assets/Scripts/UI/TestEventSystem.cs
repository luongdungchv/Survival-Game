using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestEventSystem : MonoBehaviour, IPointerClickHandler
{
    public string test;
    public Camera mainCam;
    public Canvas canvas;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(test);
    }
    private void Start()
    {
        //Debug.Log(Camera.main.Screeb)
    }
    private void Update()
    {
        var mousePosWorld = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCam.farClipPlane));
        Debug.Log(mousePosWorld);
        this.GetComponent<RectTransform>().anchoredPosition = mousePosWorld / (canvas.scaleFactor + 0.1f);
    }
}
