using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSceneUI : MonoBehaviour
{
    public static MultiSceneUI ins;
    private void Awake()
    {
        if (ins == null) ins = this;
        else
        {
            Destroy(ins.gameObject);
            ins = this;
        }
    }
}
