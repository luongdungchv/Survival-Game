using UnityEngine;
using System.Linq;

public class ItemDropObject : DamagableObject
{
    [SerializeField] private string itemName;
    private Item itemBase => Item.GetItem(itemName);
    public override void OnDamage(float incomingDmg, int inputPriority, string tool)
    {
        if (!requiredTools.Contains(tool)) return;
        if (inputPriority < priority) return;
        hp -= incomingDmg;
        if (hp <= 0)
        {
            var drop = itemBase.Drop(transform.position, 6);
            drop.SetQuantity(9);
            drop.gameObject.SetActive(true);
            Destroy(this.gameObject);
        }
    }
}