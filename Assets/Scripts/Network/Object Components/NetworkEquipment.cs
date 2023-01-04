using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class NetworkEquipment : MonoBehaviour
{
    private Item rightHandItem;

    public void SetRightHandItem(Item item)
    {
        this.rightHandItem = item;
    }
    public void Use()
    {
        if (rightHandItem != null && rightHandItem.TryGetComponent<IUsable>(out var usableItem))
        {
            usableItem.OnUse(-1);
        }
    }
}
