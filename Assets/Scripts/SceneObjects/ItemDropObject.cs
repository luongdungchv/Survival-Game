using UnityEngine;
using System.Linq;

public class ItemDropObject : MonoBehaviour, IDamagable
{
    [SerializeField] private float hp;
    [SerializeField] private string itemName;
    [SerializeField] private int minDrop, maxDrop;
    [SerializeField] private string[] requiredTools;
    private Item itemBase => Item.GetItem(itemName);
    private void OnDamage(float incomingDmg, string tool)
    {
        if (!requiredTools.Contains(tool)) return;
        //if (inputPriority < priority) return;
        hp -= incomingDmg;
        if (hp <= 0)
        {
            var drop = itemBase.Drop(transform.position + Vector3.up * 3, Random.Range(minDrop, maxDrop + 1));

            Destroy(this.gameObject);
        }
    }
    public void OnDamage(IHitData hitData)
    {
        var playerHitData = hitData as PlayerHitData;
        Debug.Log(playerHitData.damage);
        OnDamage(playerHitData.damage, playerHitData.atkTool);
    }
}