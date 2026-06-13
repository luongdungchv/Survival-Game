using UnityEngine;

namespace BoxHead2.Items
{
    public interface IEquipmentAttachable
    {
        WeaponAttachSlot FindAttachSlot(WeaponAttachSlotType configSlot);
        WeaponAttachSlot[] GetAllAttachSlot();
        Transform Transform { get; }
    }
}