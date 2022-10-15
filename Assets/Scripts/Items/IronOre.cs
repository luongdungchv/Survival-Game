using UnityEngine;
public class IronOre : Item
{
    public static IronOre ins;
    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }

}