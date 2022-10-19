using UnityEngine;

public class Rock : Item
{
    public static Rock ins;
    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }
}