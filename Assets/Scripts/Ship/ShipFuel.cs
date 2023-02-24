using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipFuel : InteractableObject
{
    [SerializeField] private Transform playerBoardPos;
    protected override void OnInteractBtnClick(Button clicker)
    {
        NetworkPlayer.localPlayer.transform.position = playerBoardPos.position;
        base.OnInteractBtnClick(clicker);
    }
}
