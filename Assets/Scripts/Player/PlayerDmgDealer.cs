using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDmgDealer : MonoBehaviour
{
    public static PlayerDmgDealer ins;
    private float baseDmg = 0;
    private string tool;
    private PlayerStats dealer;
    private IDamagable receiver;
    private bool isCrit, isKnockback;
    private void Start()
    {
        ins = this;
    }

    public void Excute()
    {
        receiver.OnDamage(new PlayerHitData(baseDmg, tool, dealer, this.isCrit, this.isKnockback));
    }
    public void SetProps(float dmg, string tool, PlayerStats dealer, IDamagable receiver, bool isCrit = false, bool isKnockback = false)
    {
        this.baseDmg = dmg;
        this.dealer = dealer;
        this.receiver = receiver;
        this.tool = tool;
        this.isCrit = isCrit;
        this.isKnockback = isKnockback;
    }
}
