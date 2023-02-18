using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chest : InteractableObject
{
    [SerializeField] private GameObject chestLid;
    [SerializeField] private int openDirection = 1;
    [SerializeField] private float maxOpenAngle, openDuration;
    [SerializeField] private int chestLevel;
    [SerializeField] private PowerupMapper powerupMapper;
    
    
    private PowerupsManager powerupsManager => PowerupsManager.instance;
    private GameObject currentClicker;
    private NetworkSceneObject netObj => GetComponentInParent<NetworkSceneObject>();
    protected override void OnInteractBtnClick(Button clicker)
    {
        Destroy(clicker.gameObject);
        var openChestPacket = new RawActionPacket(PacketType.ChestInteraction);
        openChestPacket.WriteData(Client.ins.clientId, netObj.id, "open", null);
        Client.ins.SendTCPPacket(openChestPacket);
        if(Client.ins.isHost) Open();
    }
    IEnumerator LerpOpenChest()
    {
        float t = 0;
        float lastRot = 0;
        float currentRot = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / openDuration;
            float openAngle = Mathf.Lerp(0, maxOpenAngle, t);
            currentRot = openAngle - lastRot;
            chestLid.transform.Rotate(-currentRot * openDirection, 0, 0);
            lastRot = openAngle;
            yield return null;
        }
    }
    public void Open()
    {
        StartCoroutine(LerpOpenChest());
        interactable = false;
        netObj.RevokeId();
    }
    private void RandomlyDropPowerup(){
        var availablePowerups = powerupsManager.GetPowerupListByTier(chestLevel);
        var randomIndex = Random.Range(0, availablePowerups.Count - 1);
        var chosenPowerup = availablePowerups[randomIndex];
        var dropPrefab = powerupMapper.GetDropPrefab(chosenPowerup);
        var powerupDrop = Instantiate(dropPrefab, transform.position + Vector3.up * 3, Quaternion.identity);
    }
}
