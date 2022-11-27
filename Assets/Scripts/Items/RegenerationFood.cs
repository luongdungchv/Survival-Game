using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenerationFood : Item, IConsumable, IEquippable
{
    [SerializeField] private RegenType[] regenTypes;
    [SerializeField] private float consumeDuration;
    [SerializeField] private float regenAmount;
    [SerializeField] private GameObject model;
    public float duration => consumeDuration;

    public GameObject inHandModel => model;


    public void OnConsume(int itemIndex)
    {
        bool regenAction = false;
        foreach (var regenType in regenTypes)
        {
            if (regenType == RegenType.Health)
            {
                regenAction = regenAction || PlayerStats.ins.RegenerateHP(regenAmount);
            }
            else if (regenType == RegenType.Stamina)
            {
                bool regenStamina = PlayerStats.ins.RegenerateStamina(regenAmount);
                regenAction = regenAction || regenStamina;
            }
            else if (regenType == RegenType.Hunger)
            {
                bool regenHunger = PlayerStats.ins.RegenerateHungerPoint(regenAmount);
                regenAction = regenAction || regenHunger;
            }
        }
        if (regenAction)
        {
            Inventory.ins.Remove(itemIndex, 1);
        }
    }

    public void OnEquip()
    {
        model.SetActive(true);
    }

    public void OnUnequip()
    {
        model.SetActive(false);
    }
    enum RegenType
    {
        Health, Stamina, Hunger
    }
}
