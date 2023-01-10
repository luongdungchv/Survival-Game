using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPrefab : MonoBehaviour
{
    public static int instanceCount = 0;

    private void Awake()
    {
        if (!gameObject.TryGetComponent<NetworkSceneObject>(out var netSceneObj))
        {
            this.gameObject.AddComponent<NetworkSceneObject>();
        }
    }
}
