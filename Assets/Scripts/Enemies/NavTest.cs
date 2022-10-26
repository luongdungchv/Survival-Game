using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavTest : MonoBehaviour
{
    private void Start()
    {
        GetComponent<NavMeshAgent>().destination = Vector3.up;
    }
}
