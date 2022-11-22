using UnityEngine;

public class GoldOre : Item, ITransformable
{
    public static GoldOre ins;
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