using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNearbyDetector : HitBox
{
    private FixedSizeUI displayer;
    private bool isDisplayed;
    private void Start()
    {
        displayer = GetComponent<FixedSizeUI>();
        if (displayer == null)
        {
            displayer = GetComponentInParent<FixedSizeUI>();
        }
        if (displayer == null)
        {
            displayer = GetComponentInChildren<FixedSizeUI>();
        }
    }
    protected override bool OnHitDetect(RaycastHit hit)
    {
        if (!hit.collider.GetComponent<NetworkPlayer>().isLocalPlayer) return false;
        isDisplayed = true;
        //displayer?.SetDisplay(true);
        //Debug.Log(hit.collider.gameObject.name);
        return true;
    }
    protected override void OnNoHitDetect()
    {
        isDisplayed = false;
        //displayer?.SetDisplay(false);
    }
}
