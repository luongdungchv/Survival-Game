using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public NetworkPrefab prefab;
    private void Start()
    {
        var a = Instantiate(prefab);
        a.GetComponent<NetworkSceneObject>().id = "23423";
        Debug.Log(a.GetComponent<NetworkSceneObject>().id);
    }
}
