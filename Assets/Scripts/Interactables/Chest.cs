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
    private GameObject currentClicker;
    private NetworkSceneObject netObj => GetComponentInParent<NetworkSceneObject>();
    protected override void OnInteractBtnClick(Button clicker)
    {
        Destroy(clicker.gameObject);
        var openChestPacket = new ObjectInteractionPacket(PacketType.ChestInteraction);
        openChestPacket.WriteData(Client.ins.clientId, netObj.id, "open", null);
        Client.ins.SendTCPPacket(openChestPacket);
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
}
