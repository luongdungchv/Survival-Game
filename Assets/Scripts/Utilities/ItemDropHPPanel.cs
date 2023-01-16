using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropHPPanel : MonoBehaviour
{
    private Quaternion originalRotation;
    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation * originalRotation;
    }
}
