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
    [SerializeField] private int chestLevel, baseCost = 2;
    [SerializeField] private PowerupMapper powerupMapper;
    [SerializeField] private GameObject foodDropPrefab;

    protected override void Start()
    {
        base.Start();
        this.displayName += $" {baseCost * chestLevel} coins";
    }

    private PowerupsManager powerupsManager => PowerupsManager.instance;
    private GameObject currentClicker;
    private NetworkSceneObject netObj => GetComponentInParent<NetworkSceneObject>();
    protected override void OnInteractBtnClick(Button clicker)
    {
        Destroy(clicker.gameObject);
        var openChestPacket = new RawActionPacket(PacketType.ChestInteraction);
        var coins = NetworkPlayer.localPlayer.GetComponent<PlayerStats>().coins;
        if (coins >= baseCost * chestLevel)
        {
            openChestPacket.WriteData(Client.ins.clientId, netObj.id, "open", null);
            Client.ins.SendTCPPacket(openChestPacket);
            if (Client.ins.isHost)
            {
                Open(NetworkPlayer.localPlayer);
            }
        }
    }
    IEnumerator LerpOpenChest(NetworkPlayer opener)
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
        if(Client.ins.isHost){
            DropFood();
            
        }
        opener.GetComponent<PlayerStats>().coins -= baseCost * chestLevel;
    }
    public void Open(NetworkPlayer opener)
    {
        StartCoroutine(LerpOpenChest(opener));
        interactable = false;
        netObj.RevokeId();
    }
    private void DropFood()
    {
        var foodDrop = Item.GetItem("food").Drop(transform.position + Vector3.up * 2, chestLevel + 1);
        // var dropComponent = foodDrop.GetComponentInChildren<ItemDrop>();
        // dropComponent.SetQuantity(chestLevel + 1);
        // dropComponent.SetBase(Item.GetItem("food"));
        var randX = Random.Range(-1f, 1f);
        var randZ = Random.Range(-1f, 1f);
        var randDir = new Vector3(randX, 1, randZ);
        foodDrop.GetComponentInParent<Rigidbody>().AddForce(randDir, ForceMode.Impulse);

    }
    private void RandomlyDropPowerup()
    {
        var availablePowerups = powerupsManager.GetPowerupListByTier(chestLevel);
        var randomIndex = Random.Range(0, availablePowerups.Count - 1);
        var chosenPowerup = availablePowerups[randomIndex];
        var dropPrefab = powerupMapper.GetDropPrefab(chosenPowerup);
        var powerupDrop = Instantiate(dropPrefab, transform.position + Vector3.up * 3, Quaternion.identity);
    }
}
