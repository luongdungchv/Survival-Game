using System;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Item, IUsable, IEquippable, ICraftable
{
    public static Sword ins;
    [SerializeField] private GameObject _inHandModel;
    [SerializeField] private List<MaterialList> requiredMatList;
    private Dictionary<string, int> _requiredMats;
    public Dictionary<string, int> requiredMats => _requiredMats;

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
    public void OnUse()
    {
        var equipmentSystem = PlayerEquipment.ins;
        var atkSystem = PlayerAttack.ins;
        atkSystem.SetAtkPattern(equipmentSystem.swordAtkPattern);
        //atkSystem.PerformAttack();
        StateMachine.ins.ChangeState("Attack");
    }
    [Serializable]
    class MaterialList
    {
        public string name;
        public int quantity;
    }
}