using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    BoxCollider detector;
    private void Start()
    {
        detector = GetComponent<BoxCollider>();
    }

    public void EnableWeapon()
    {
        detector.enabled = true;
    }
    public void DisableWeapon()
    {
        detector.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("hit");
        }
        DisableWeapon();
    }
}
