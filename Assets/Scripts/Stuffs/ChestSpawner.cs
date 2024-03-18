﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [SerializeField] private int maxChestPerEdge, castHeight;
    [SerializeField] private LayerMask mask;
    [SerializeField] private List<ChestType> chestTypes;

    public void Init()
    {
        var randObj = new CustomRandom(MapGenerator.ins.seed);
        bool[,] occupationMap = new bool[maxChestPerEdge, maxChestPerEdge];
        RaycastHit hit;

        foreach (var type in chestTypes)
        {
            for (int i = 0; i < type.quantity; i++)
            {
                var randX = randObj.Next(0, maxChestPerEdge - 1);
                var randY = randObj.Next(0, maxChestPerEdge - 1);
                while (occupationMap[randX, randY])
                {
                    randX = randObj.Next(0, maxChestPerEdge - 1);
                    randY = randObj.Next(0, maxChestPerEdge - 1);
                }
                float regionSize = 1500 / (maxChestPerEdge - 1);
                var posX = randX * regionSize + regionSize / 2;
                var posY = randY * regionSize + regionSize / 2;
                var castPos = new Vector3(posX, castHeight, posY);
                if (Physics.Raycast(castPos, Vector3.down, out hit, 400, mask))
                {
                    if (hit.collider.tag != "Terrain") continue;
                    var rotateSlope = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    var chest = Instantiate(type.prefab, hit.point, rotateSlope);
                    var netObj = chest.AddComponent<NetworkObject>();
                    chest.transform.Rotate(0, randObj.NextFloat(0, 360), 0);
                    MapGenerator.ins.AddObjToStaticBatching(chest.transform);
                }
            }
        }

    }

    void Update()
    {

    }
    [System.Serializable]
    class ChestType
    {
        public string name;
        public GameObject prefab;
        public float quantity;
    }
}
