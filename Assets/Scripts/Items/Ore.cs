using UnityEngine;

public class Ore : Item, ITransformable
{
    public static Ore ins;
    [SerializeField] private Item transformItem;
    [SerializeField] private int requiredUnits;

    public Item goalItem => transformItem;

    public int cookability => requiredUnits;

    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }


}