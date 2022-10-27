using UnityEngine;

public class GoldOre : Item
{
    public static GoldOre ins;
    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }

}