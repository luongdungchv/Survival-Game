using UnityEngine;

public class Wood : Item, IFuel
{
    public static Wood ins;

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