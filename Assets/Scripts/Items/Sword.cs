using UnityEngine;

public class Sword : Item, IUsable
{
    public static Sword ins;
    protected override void Awake()
    {
        if (ins == null) ins = this;
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
}