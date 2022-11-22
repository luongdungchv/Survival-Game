using UnityEngine;

public class CoalOre : Item, IFuel
{
    public static CoalOre ins;
    [SerializeField] private float _efficiency;
    [SerializeField] private int _durability;
    public float efficiency => _efficiency;

    public int durability => _durability;

    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }

}