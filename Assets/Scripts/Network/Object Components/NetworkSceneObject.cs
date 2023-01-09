using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSceneObject : NetworkObject
{
    private void Start()
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
    private void OnDestroy()
    {
        RevokeId();
    }
}
