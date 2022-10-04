using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private BoxCollider detector;
    private void Start()
    {
        //detector = GetComponent<BoxCollider>();
    }

    public void EnableWeapon()
    {
        detector.enabled = true;
        //transform.parent.gameObject.SetActive(true);
    }
    public void DisableWeapon()
    {
        detector.enabled = false;
        //transform.parent.gameObject.SetActive(false);
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
