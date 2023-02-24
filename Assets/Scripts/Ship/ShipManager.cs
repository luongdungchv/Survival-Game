using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Events;

public class ShipManager : MonoBehaviour
{
    public static ShipManager ins;
    public UnityEvent OnRepairItemAdd;
    [SerializeField] private List<RepairDetail> repairDetailList;
    private Dictionary<Item, int> repairDetails;
    private Dictionary<Item, int> currentDetails;

    private void Awake() {
        ins = this;
        OnRepairItemAdd = new UnityEvent();
        foreach(var i in repairDetailList){
            repairDetails.Add(i.item, i.quantity);
            currentDetails.Add(i.item, 0);
        }
    }

    // Update is called once per frame
    public bool AddItem(Item repairItem, int quantity){
        if(!currentDetails.ContainsKey(repairItem)) return false;
        currentDetails[repairItem] += quantity;
        CheckVictory();
        return true;
    }
    private void CheckVictory(){
        foreach(var i in repairDetails.Keys){
            if(repairDetails[i] > currentDetails[i]){
                Debug.Log("Not victory yet");
                return;
            }
        }
        Debug.Log("Victory");
    }
    public class RepairDetail{
        public Item item;
        public int quantity;
    }
}
