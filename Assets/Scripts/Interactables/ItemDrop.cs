using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ItemDrop : InteractableObject
{
    [SerializeField] private int quantity;
    public Texture2D meshTex;
    public Color outlineColor;
    [SerializeField] private Item itemBase;
    [SerializeField] private bool showOutline = true;
    protected override void Awake()
    {
        base.Awake();

    }
    protected override void Start()
    {
        base.Start();
        var renderer = this.GetComponentInParent<Renderer>();
        if (meshTex != null)
            renderer.material.mainTexture = meshTex;
        if (outlineColor != null && showOutline)
        {
            renderer.material.SetColor("_OutlineColor", outlineColor);
        }
    }

    protected override void OnInteractBtnClick(Button clicker)
    {
        base.OnInteractBtnClick(clicker);
        if (Inventory.ins.Add(itemBase, quantity))
        {
            var netSceneObj = GetComponentInParent<NetworkSceneObject>();
            netSceneObj.DestroyObject();
            Debug.Log(netSceneObj.id);
            Destroy(this.transform.parent.gameObject);
        }

    }
    public void SetQuantity(int quantity)
    {
        this.quantity = quantity;
    }
    public void SetBase(Item itemBase)
    {
        this.itemBase = itemBase;
    }
}
