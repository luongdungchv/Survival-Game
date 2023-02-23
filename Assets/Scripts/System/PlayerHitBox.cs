//using System;
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
            var dmgDealer = GetComponentInParent<PlayerStats>();
            
            var isCrit = GetCritHit(dmgDealer, currentBaseDmg);
            var isKnockback = GetKnockback(dmgDealer);
            
            if (Client.ins.isHost)
            {
                PlayerDmgDealer.ins.SetProps(currentBaseDmg, currentTool, dmgDealer, target);
                PlayerDmgDealer.ins.Excute();
            }

            var hitPacket = new RawActionPacket(PacketType.ItemDropObjInteraction);
            hitPacket.playerId = Client.ins.clientId;

            hitPacket.objId = hit.collider.GetComponent<NetworkSceneObject>().id;
            hitPacket.action = "take_dmg";
            hitPacket.actionParams = new string[]{
                    currentTool,
                    currentBaseDmg.ToString(),
                    (isCrit ? 1 : 0).ToString(),
                    (isKnockback ? 1 : 0).ToString(),
                };

            Client.ins.SendTCPPacket(hitPacket);

            return true;
        }
        return false;
    }
    private bool GetCritHit(PlayerStats dealer, float baseDmg){
        float chosenCritRate = 5;
        float chosenCritDmg = 150;
        if (tool.Contains("_sword"))
        {
            chosenCritRate = dealer.enemyCritRate;
            chosenCritDmg = dealer.enemyCritDmg;
        }
        else if (tool.Contains("_axe"))
        {
            chosenCritRate = dealer.treeCritRate;
            chosenCritDmg = dealer.treeCritDmg;
        }
        else if (tool.Contains("_pickaxe"))
        {
            chosenCritRate = dealer.oreCritRate;
            chosenCritDmg = dealer.oreCritDmg;
        }
        var prob = Random.Range(0, 10000);
        var isCrit = false;
        if (prob <= chosenCritRate * 100)
        {
            baseDmg *= chosenCritDmg / 100;
            baseDmg = Mathf.Round(baseDmg);
            isCrit = true;
        }
        return isCrit;
    }
    private bool GetKnockback(PlayerStats dealer){
        var knockbackRate = dealer.knockbackRate;
        var isKnockback = Random.Range(1, 101) <= knockbackRate;
        return isKnockback;
    }
}