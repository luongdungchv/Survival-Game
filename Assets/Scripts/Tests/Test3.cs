﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Test3 : MonoBehaviour
{
    Collider hitbox;
    Vector3 center;
    Vector3 halfExtents;
    public LayerMask mask;
    // Start is called before the first frame update
    private void Awake()
    {
        //Destroy(this.gameObject);
    }
    void Start()
    {
        Debug.Log("start");
        hitbox = this.GetComponent<Collider>();
        center = hitbox.bounds.center;
        halfExtents = hitbox.bounds.extents;
        Debug.Log(this.GetComponent<Collider>().bounds.size);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnDrawGizmos()
    {

    }

}
