using UnityEngine;
using System.Linq;

public class ItemDropObject : MonoBehaviour, IDamagable
{
    [SerializeField] private float hp;
    [SerializeField] private string itemName;
    [SerializeField] private int minDrop, maxDrop;
    [SerializeField] private string[] requiredTools;
    private Item itemBase => Item.GetItem(itemName);
    private float maxHP;
    private ObjectPool<DamagePopup> dmgPopupPool => PoolManager.ins.dmgPopupPool;
    private void Start()
    {
        maxHP = hp;
    }
    private void OnDamage(PlayerStats player, float incomingDmg, string tool, bool isCrit)
    {
        if (!requiredTools.Contains(tool)) incomingDmg = 0;
        //if (inputPriority < priority) return;
        hp -= incomingDmg;
        var isDealerLocalPlayer = player.GetComponent<NetworkPlayer>().isLocalPlayer;
        FixedSizeUI hpBar;
        if ((TryGetComponent<FixedSizeUI>(out hpBar)) && isDealerLocalPlayer)
        {
            hpBar.SetElementValue(Mathf.InverseLerp(0, maxHP, hp));
            var popup = dmgPopupPool.Release();
            var critLevel = isCrit ? 1 : 0;
            popup.Popup(hpBar.uiWorldPos.position, incomingDmg.ToString(), critLevel);
        }
        if (hp <= 0)
        {
            if (Client.ins.isHost)
            {
                var drop = itemBase.Drop(transform.position + Vector3.up * 3, Random.Range(minDrop, maxDrop + 1));
            }
            Destroy(this.gameObject);
        }
    }
    public void OnDamage(IHitData hitData)
    {
        var playerHitData = hitData as PlayerHitData;
        OnDamage(playerHitData.dealer, playerHitData.damage, playerHitData.atkTool, playerHitData.crit);
    }
}