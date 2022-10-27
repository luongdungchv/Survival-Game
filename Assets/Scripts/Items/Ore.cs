using UnityEngine;

public class Ore : Item
{
    public static Ore ins;
    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }

}