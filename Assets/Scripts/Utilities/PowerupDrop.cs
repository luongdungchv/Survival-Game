using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupDrop : InteractableObject
{
    public Powerup powerup;
    private PowerupsManager manager => PowerupsManager.instance;
    private NetworkSceneObject netSceneObj;
    private void Start() {
        netSceneObj = GetComponent<NetworkSceneObject>();     
    }
    protected override void OnInteractBtnClick(Button clicker)
    {
        base.OnInteractBtnClick(clicker);
        manager.AddPowerup(powerup);
        netSceneObj.DestroyObject();
        Destroy(this.gameObject);
    }
    public void SetPowerup(Powerup input){
        this.powerup = input;
    }
}
