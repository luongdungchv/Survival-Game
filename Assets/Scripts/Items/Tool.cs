using System;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item, IUsable, IEquippable, ICraftable
{
    public static Tool ins;
    [SerializeField] private GameObject _inHandModel;
    [SerializeField] private List<MaterialList> requiredMatList;
    [SerializeField] private AttackPattern atkPattern;

    private Dictionary<string, int> _requiredMats;
    public Dictionary<string, int> requiredMats => _requiredMats;
    public float baseDmg;

    public GameObject inHandModel { get => _inHandModel; }

    protected override void Awake()
    {
        if (ins == null) ins = this;
        _requiredMats = new Dictionary<string, int>();
        foreach (var i in requiredMatList)
        {
            requiredMats.Add(i.name, i.quantity);
        }
        base.Awake();
    }
    public void OnUse(int itemIndex)
    {
        var equipmentSystem = PlayerEquipment.ins;
        var atkSystem = PlayerAttack.ins;
        atkSystem.SetAtkPattern(atkPattern);
        PlayerEquipment.ins.GetComponent<StateMachine>().ChangeState("Attack");
    }
    public void OnEquip()
    {
        inHandModel.SetActive(true);
        //PlayerAttack.ins.currentWieldName = this.itemName;
        PlayerAttack.ins.currentWield = this;
    }
    public void OnUnequip()
    {
        inHandModel.SetActive(false);
        //PlayerAttack.ins.currentWieldName = "";
    }

}
