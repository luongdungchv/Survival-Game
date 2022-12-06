using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Chest : InteractableObject
{
    [SerializeField] private GameObject chestLid;
    [SerializeField] private int openDirection = 1;
    [SerializeField] private float maxOpenAngle, openDuration;
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
            chestLid.transform.Rotate(-currentRot * openDirection, 0, 0);
            lastRot = openAngle;
            yield return null;
        }
    }
}
