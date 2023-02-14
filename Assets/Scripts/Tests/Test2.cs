using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    Rigidbody rb => GetComponent<Rigidbody>();
    private void FixedUpdate() {
        Debug.Log(rb.position);
    }
}
