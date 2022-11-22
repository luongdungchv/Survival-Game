using UnityEngine;
public class IronOre : Item, ITransformable
{
    public static IronOre ins;
    [SerializeField] private Item _transformItem;
    [SerializeField] private int _cookability;

    public Item goalItem => _transformItem;

    public int cookability => _cookability;

    protected override void Awake()
    {
        if (ins == null) ins = this;
        base.Awake();
    }

}