using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDmgDealer : MonoBehaviour
{
    public static PlayerDmgDealer ins;
    private float baseDmg = 0;
    private string tool;
    private HitBox dealer;
    private DamagableObject receiver;
    private IHitData data;
    private void Start()
    {
        ins = this;
    }

    public void Excute()
    {
        //Debug.Log(baseDmg);
        receiver.OnDamage(new PlayerHitData(baseDmg, tool, dealer));
    }
    public void SetProps(float dmg, string tool, HitBox dealer, DamagableObject receiver)
    {
        this.baseDmg = dmg;
        this.dealer = dealer;
        this.receiver = receiver;
        this.tool = tool;
    }
}
