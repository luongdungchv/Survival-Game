using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Test3 : MonoBehaviour
{
    Rigidbody rb => GetComponent<Rigidbody>();
    private void FixedUpdate()
    {
        Debug.Log(rb.position);
        GetComponent<Rigidbody>().linearVelocity = Vector3.right * 7;
    }
}
