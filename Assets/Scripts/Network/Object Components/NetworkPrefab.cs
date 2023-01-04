using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPrefab : MonoBehaviour
{
    public static int instanceCount = 0;
    public NetworkPrefab()
    {
        instanceCount++;
    }
    ~NetworkPrefab()
    {
        instanceCount--;
    }
    private void OnValidate()
    {

    }
}
