using System.Collections.Generic;
using UnityEngine;

public class CraftTable : Item, IEquippable, IUsable, ICraftable
{
    public static CraftTable ins;
    [SerializeField] private GameObject placerObj;

    public GameObject inHandModel => placerObj;
    [SerializeField] private List<MaterialList> requiredMatList;

    private Dictionary<string, int> _requiredMats;
    public Dictionary<string, int> requiredMats => _requiredMats;

    public void OnUse(int itemIndex)
    {
        var placer = placerObj.GetComponent<Placer>();
        placer.ConfirmPosition();
        Inventory.ins.Remove(itemIndex, 1);
    }
    public void OnEquip()
    {
        inHandModel.SetActive(true);
    }
    public void OnUnequip()
    {
        inHandModel.SetActive(false);
    }
    public void OnUse(NetworkPlayer netUser)
    {

    }
    protected override void Awake()
    {
        if (ins == null) ins = this;
        _requiredMats = new Dictionary<string, int>();
        foreach (var i in requiredMatList)
        {
            requiredMats.Add(i.name, i.quantity);
        }
        base.Awake();
    }

}