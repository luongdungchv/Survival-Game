using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEquipment : MonoBehaviour
{
    private Item rightHandItem;
    [SerializeField] private ItemModel[] itemMapperList;
    private Dictionary<string, GameObject> itemMapper;
    private NetworkPlayer netPlayer;
    private void Awake()
    {
        netPlayer = GetComponent<NetworkPlayer>();
        itemMapper = new Dictionary<string, GameObject>();
        foreach (var i in itemMapperList)
        {
            itemMapper.Add(i.item, i.model);
        }
    }
    public void SetRightHandItem(Item item)
    {

        if (rightHandItem != null) itemMapper[rightHandItem.itemName].SetActive(false);
        this.rightHandItem = item;
        itemMapper[rightHandItem.itemName].SetActive(true);
    }
    public void Use()
    {
        if (rightHandItem != null && rightHandItem.TryGetComponent<IUsable>(out var usableItem))
        {
            usableItem.OnUse(netPlayer);
        }
    }
    [System.Serializable]
    class ItemModel
    {
        public string item;
        public GameObject model;
    }
}
