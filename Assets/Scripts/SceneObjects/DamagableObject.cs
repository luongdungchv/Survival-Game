using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamagableObject : MonoBehaviour
{
    [SerializeField] private float hp;
    [SerializeField] private int priority;
    [SerializeField] private string[] requiredTools;
    [SerializeField] private CollectableItem dropItem;
    // Start is called before the first frame update

    public virtual void OnDamage(float incomingDmg, int inputPriority, string tool)
    {
        if (!requiredTools.Contains(tool)) return;
        if (inputPriority < priority) return;
        hp -= incomingDmg;
        if (hp <= 0)
        {
            Instantiate(dropItem.gameObject, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
