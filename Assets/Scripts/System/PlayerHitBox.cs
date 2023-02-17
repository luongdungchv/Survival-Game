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
            string currentTool = PlayerAttack.ins.currentWieldName;
            float currentBaseDmg = PlayerAttack.ins.currentBaseDmg;
            if (Client.ins.isHost)
            {
                PlayerDmgDealer.ins.SetProps(currentBaseDmg, currentTool, GetComponentInParent<PlayerStats>(), target);
                PlayerDmgDealer.ins.Excute();
            }

            var hitPacket = new RawActionPacket(PacketType.ItemDropObjInteraction);
            hitPacket.playerId = Client.ins.clientId;

            hitPacket.objId = hit.collider.GetComponent<NetworkSceneObject>().id;
            hitPacket.action = "take_dmg";
            hitPacket.actionParams = new string[]{
                    currentTool,
                    currentBaseDmg.ToString(),
                };

            Client.ins.SendTCPPacket(hitPacket);

            return true;
        }
        return false;
    }
}