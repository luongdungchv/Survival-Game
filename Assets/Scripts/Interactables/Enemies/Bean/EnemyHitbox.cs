using System.Collections;
using System.Collections.Generic;
using Enemy.Base;
using UnityEngine;

public class EnemyHitbox : HitBox
{
    private EnemyStats stats;
    private float damage => stats.baseDamage;
    private void Start()
    {
        stats = GetComponentInParent<EnemyStats>();
    }

    protected override bool OnHitDetect(RaycastHit hit)
    {
        if (hit.collider.tag == "Player")
        {
            var netPlayer = hit.collider.GetComponent<NetworkPlayer>();
            if (!netPlayer.isLocalPlayer) return false;
            var playerStats = hit.collider.GetComponent<PlayerStats>();
            if (Client.ins.isHost)
            {
                playerStats.TakeDamage(damage);
            }

            var dmgPlayerPacket = new RawActionPacket(PacketType.PlayerInteraction)
            {
                action = "take_damage",
                playerId = netPlayer.id,
                actionParams = new string[] { damage.ToString() }
            };
            Client.ins.SendTCPPacket(dmgPlayerPacket);

            return true;
        }
        return false;
    }
}
