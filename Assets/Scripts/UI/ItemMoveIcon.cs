using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemMoveIcon : MonoBehaviour
{
    [SerializeField] RawImage iconImage;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] private Camera mainCam;
    public float farplane;
    public Canvas canvas;
    public Texture2D _icon;
    public Texture2D icon
    {
        get => _icon;
        set
        {
            _icon = value;
            iconImage.texture = _icon;
        }
    }
    public int _quantity;
    public int quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            quantityText.text = _quantity.ToString();
            if (value > 0) iconImage.gameObject.SetActive(true);
            else iconImage.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        FollowMouse();
    }
    private void Update()
    {
        FollowMouse();
    }
    private void FollowMouse()
    {
        var mousePosWorld = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCam.farClipPlane));

        //mousePosWorld -= mainCam.transform.position;
        mousePosWorld = mainCam.transform.worldToLocalMatrix.MultiplyPoint(mousePosWorld);

        //mousePosWorld = (mousePosWorld.normalized / Vector3.Dot(mousePosWorld.normalized, mainCam.transform.forward) - mainCam.transform.forward) * mainCam.farClipPlane;
        //Debug.Log(mousePosWorld.magnitude);
        Debug.DrawLine(mainCam.transform.position, mainCam.transform.position + mousePosWorld, Color.magenta);
        mousePosWorld = mousePosWorld - Vector3.forward * mainCam.farClipPlane;
        Vector2 iconPos = new Vector2(mousePosWorld.x, mousePosWorld.y);
        Debug.Log(mousePosWorld.z);

        this.GetComponent<RectTransform>().anchoredPosition = iconPos / (canvas.scaleFactor + 0.1f);
    }


}
