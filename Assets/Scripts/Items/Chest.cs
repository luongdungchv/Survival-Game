using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Chest : InteractableItem
{
    [SerializeField] private GameObject chestLid;
    [SerializeField] private float maxOpenAngle, openDuration;
    public UnityEvent testEvent;
    protected override void OnInteractBtnClick(Button clicker)
    {
        StartCoroutine(LerpOpenChest());
        interactable = false;
        Destroy(clicker.gameObject);
    }
    IEnumerator LerpOpenChest()
    {
        float t = 0;
        float lastRot = 0;
        float currentRot = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / openDuration;
            float openAngle = Mathf.Lerp(0, maxOpenAngle, t);
            currentRot = openAngle - lastRot;
            chestLid.transform.Rotate(-currentRot, 0, 0);
            lastRot = openAngle;
            yield return null;
        }
    }
}
