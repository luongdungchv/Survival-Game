using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour, IUsable
{
    [SerializeField] private float regenAmount;
    public void OnUse(int itemIndex)
    {

    }
    public void OnUse(NetworkPlayer netUser)
    {

    }


}
