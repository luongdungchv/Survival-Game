using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PowerupsManager : MonoBehaviour
{
    public static PowerupsManager instance;
    [SerializeField] private List<Powerup> listOfPowerup;
    
    private Dictionary<int, List<Powerup>> groupedPowerups;
    public UnityEvent<Powerup, int> OnPowerupPickedUp;
    public UnityEvent<Powerup, int> OnPowerupRemoved;
    private Dictionary<Powerup, int> currentPowerups;
    
    private Dictionary<int, List<Powerup>> allPowerups => groupedPowerups;
    private void Awake() {
        if(instance == null) instance = this;
    }
    private void Start() {
        currentPowerups = new Dictionary<Powerup, int>();
        groupedPowerups = GroupPowerupsByTier();
    }
    public void AddPowerup(Powerup input){
        if(currentPowerups.ContainsKey(input)) currentPowerups[input]++;
        else currentPowerups.Add(input, 1);
        OnPowerupPickedUp.Invoke(input, currentPowerups[input]);
    }
    public int GetPowerupQuantity(Powerup input){
        return currentPowerups[input];
    }
    public List<Powerup> GetPowerupListByTier(int tier){
        return groupedPowerups[tier];
    }
    private Dictionary<int, List<Powerup>> GroupPowerupsByTier(){
        var res = new Dictionary<int, List<Powerup>>();
        foreach(var powerup in listOfPowerup){
            int tier = powerup.tier;
            if(res.ContainsKey(tier)) res[tier].Add(powerup);
            else res.Add(tier, new List<Powerup>(){powerup});
        }
        return res;
    }
}

