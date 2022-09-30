using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public bool b;
    // Start is called before the first frame update
    void Start()
    {
        b = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (b) Debug.Log("b");    
    }
}
