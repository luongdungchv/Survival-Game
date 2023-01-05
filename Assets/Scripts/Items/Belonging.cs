using UnityEngine;
using System.Collections.Generic;

public class Belonging : Item, IEquippable, IUsable, ICraftable
{
    public static Belonging ins;
    [SerializeField] private GameObject placerObj;
    [SerializeField] private List<MaterialList> requiredMatList;
    private Dictionary<string, int> _requiredMats;
    public Dictionary<string, int> requiredMats => _requiredMats;

    public GameObject inHandModel => placerObj;

    public void OnUse(int itemIndex)
    {
        var placer = placerObj.GetComponent<Placer>();
        placer.ConfirmPosition();
        Inventory.ins.Remove(itemIndex, 1);
    }
    public void OnUse(NetworkPlayer netUser)
    {

    }
    public void OnEquip()
    {
        placerObj.SetActive(true);
        if (!Client.ins.isHost)
        {
            var packet = new UpdateEquippingPacket();
            packet.WriteData(Client.ins.clientId, this.itemName);
            Client.ins.SendTCPPacket(packet);
        }
    }
    public void OnUnequip()
    {
        placerObj.SetActive(false);
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