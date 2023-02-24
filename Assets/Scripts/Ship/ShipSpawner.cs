using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ship;
    private CustomRandom randomObject;
    
    private List<Tuple<Vector2Int, Vector2Int>> boundMap;
    void Start()
    {
        var seed = MapGenerator.ins.seed;
        randomObject = new CustomRandom(seed);
        
        var bound_1 = new Vector2Int(-100, 1600);
        var bound_2 = new Vector2Int(-100, 0);
        var bound_3 = new Vector2Int(1500, 1600);
        
        
        boundMap = new List<Tuple<Vector2Int, Vector2Int>>();
        boundMap.Add(new Tuple<Vector2Int, Vector2Int>(bound_1, bound_2));
        boundMap.Add(new Tuple<Vector2Int, Vector2Int>(bound_1, bound_3));
        boundMap.Add(new Tuple<Vector2Int, Vector2Int>(bound_2, bound_1));
        boundMap.Add(new Tuple<Vector2Int, Vector2Int>(bound_3, bound_1));
        
        SpawnRandomly();
    }
    private void SpawnRandomly(){
        var randomPos = this.GetRandomPos();
        var randomRotation = Quaternion.Euler(0, randomObject.Next(0, 360), 0);
        ship.transform.position = randomPos;
        ship.transform.rotation = randomRotation;
        Debug.Log(randomPos);
    }
    private Vector3 GetRandomPos(){
        var randomBound = boundMap[randomObject.Next(0, 4)];
        var xBound = randomBound.Item1;
        var yBound = randomBound.Item2;
        
        var randX = randomObject.Next(xBound.x, xBound.y + 1);
        var randY = randomObject.Next(yBound.x, yBound.y + 1);
        
        return new Vector3(randX,11.14386f, randY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
