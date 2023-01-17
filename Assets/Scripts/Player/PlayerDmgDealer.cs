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
    private void Start()
    {
        ins = this;
    }

    public void Excute()
    {
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
        receiver.OnDamage(new PlayerHitData(baseDmg, tool, dealer, isCrit));
    }
    public void SetProps(float dmg, string tool, PlayerStats dealer, IDamagable receiver)
    {
        this.baseDmg = dmg;
        this.dealer = dealer;
        this.receiver = receiver;
        this.tool = tool;
    }
}
