﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSceneObject : NetworkObject
{

    private void Start()
    {
        if (this.id != null && this.id != "") return;
        GenerateId();
    }
    public void GenerateId()
    {
        this.id = GameFunctions.ins.GenerateId();
        NetworkManager.ins.AddNetworkSceneObject(this.id, this);
    }

    public void RevokeId()
    {
        GameFunctions.ins.RevokeId(this.id);
        NetworkManager.ins.RemoveSceneNetworkObject(this.id);
        this.id = null;
    }
    public void DestroyObject()
    {
        var destroyPacket = new ObjectInteractionPacket(PacketType.DestroyObject)
        {
            playerId = Client.ins.clientId,
            objId = this.id,
            action = "",
            actionParams = new string[0]
        };
        //Debug.Log("Destroy msg to send: " + destroyPacket.GetString());
        Client.ins.SendTCPPacket(destroyPacket);
    }
    private void OnDestroy()
    {
        RevokeId();
    }
}
