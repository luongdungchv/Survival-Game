using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAnimEvents : MonoBehaviour
{

    public void DashAtk()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * 10;
    }
}
