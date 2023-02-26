using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSceneObject : NetworkObject
{

    private void Start()
    {
        if (this.id != null && this.id != "") return;
        GenerateId();
    }
    public string GenerateId()
    {
        this.id = GameFunctions.GenerateId();
        NetworkManager.ins.AddNetworkSceneObject(this.id, this);
        return this.id;
    }

    public void RevokeId()
    {
        GameFunctions.RevokeId(this.id);
        NetworkManager.ins.RemoveSceneNetworkObject(this.id);
        this.id = null;
    }
    public void DestroyObject()
    {
        var destroyPacket = new RawActionPacket(PacketType.DestroyObject)
        {
            playerId = Client.ins.clientId,
            objId = this.id,
            action = "",
            actionParams = new string[0]
        };
        Client.ins.SendTCPPacket(destroyPacket);
    }
    private void OnDestroy()
    {
        RevokeId();
    }
}
