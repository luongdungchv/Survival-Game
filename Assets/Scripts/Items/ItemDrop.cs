using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ItemDrop : InteractableObject
{
    [SerializeField] private int quantity;
    [SerializeField] private Texture2D meshTex;
    [SerializeField] private Color outlineColor;
    [SerializeField] private Item itemBase;
    protected override void Awake()
    {
        base.Awake();
        var renderer = this.GetComponentInParent<Renderer>();
        if (meshTex != null)
            renderer.material.mainTexture = meshTex;
        if (outlineColor != null)
        {
            renderer.material.SetColor("_OutlineColor", outlineColor);
        }
    }

    protected override void OnInteractBtnClick(Button clicker)
    {
        base.OnInteractBtnClick(clicker);
        if (Inventory.ins.Add(itemBase, quantity))
            Destroy(this.transform.parent.gameObject);
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
