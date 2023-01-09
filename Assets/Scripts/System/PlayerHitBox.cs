using System;
using UnityEngine;

public class PlayerHitBox : HitBox
{
    [SerializeField] private string tool;

    protected override bool OnHitDetect(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent<IDamagable>(out var target))
        {
            hitVfx.transform.position = hit.point;
            hitVfx.Play();
            CamShake.ins.Shake();
            if (Client.ins.isHost)
            {
                string currentTool = PlayerAttack.ins.currentWieldName;
                float currentBaseDmg = PlayerAttack.ins.currentBaseDmg;
                PlayerDmgDealer.ins.SetProps(currentBaseDmg, currentTool, GetComponentInParent<PlayerStats>(), target);
                PlayerDmgDealer.ins.Excute();
            }
            else
            {
                var hitPacket = new ObjectInteractionPacket(PacketType.TreeInteraction);
                hitPacket.playerId = Client.ins.clientId;

                string currentTool = PlayerAttack.ins.currentWieldName;
                float currentBaseDmg = PlayerAttack.ins.currentBaseDmg;
                hitPacket.objId = hit.collider.GetComponent<NetworkSceneObject>().id;
                hitPacket.actionParams = new string[]{
                    currentTool,
                    currentBaseDmg.ToString(),
                };

                Client.ins.SendTCPPacket(hitPacket);
            }
            return true;
        }
        return false;
    }
}