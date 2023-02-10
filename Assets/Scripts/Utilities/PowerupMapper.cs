using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupMapper : ScriptableObject
{
    public List<PowerupDrop> powerupDrops;
    private Dictionary<Powerup, PowerupDrop> map;
    private void OnEnable() {
        if(map == null){
            map = new Dictionary<Powerup, PowerupDrop>();
            foreach(var drop in powerupDrops){
                map.Add(drop.powerup, drop);
            }
        }
    }
    public PowerupDrop GetDropPrefab(Powerup input) => map[input];
}
