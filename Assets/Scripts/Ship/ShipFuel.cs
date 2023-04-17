using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipFuel : InteractableObject
{
    public static ShipFuel ins;
    protected override void Awake() {
        base.Awake();
        ins = this;
    }
    [SerializeField] private Transform playerBoardPos;
    protected override void OnInteractBtnClick(Button clicker)
    {
        TeleportToShip();
        base.OnInteractBtnClick(clicker);
    }
    public void TeleportToShip(){
        NetworkPlayer.localPlayer.GetComponent<StateMachine>().ChangeState("Idle", true);
        NetworkPlayer.localPlayer.transform.position = playerBoardPos.position;
    }
}
