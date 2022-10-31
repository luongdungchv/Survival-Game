using UnityEngine;

public class Wood : Item
{
    public static Wood ins;
    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }

}