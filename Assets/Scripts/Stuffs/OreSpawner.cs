﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreSpawner : MonoBehaviour
{
    public int cellDist;
    [SerializeField] private List<OreType> oreTypes;
    [SerializeField] private float castHeight;
    [SerializeField] private LayerMask mask;
    //[SerializeField] private GameObject cherryPetalParticle;
    private Dictionary<Vector2Int, bool> regionsOccupation;
    private int seed;
    // Start is called before the first frame update
    void Start()
    {
        regionsOccupation = new Dictionary<Vector2Int, bool>();
        RaycastHit hit;
        seed = GetComponent<MapGenerator>().seed;

        var terrainTypes = GetComponent<MapGenerator>().terrainTypes;
        var maxHeight = GetComponent<MapGenerator>().vertMaxHeight;
        var waterHeight = terrainTypes[terrainTypes.Count - 2].height + 0.05f;
        var skipHeight = waterHeight * maxHeight;
        var randObj = new CustomRandom(seed);

        foreach (var i in oreTypes)
        {
            for (int a = 0; a < i.regionCount; a++)
            {
                if (regionsOccupation.Count >= 100) break;
                var randPos = GetRandomPos(randObj);

                randPos = new Vector2Int(randPos.x * cellDist, randPos.y * cellDist);

                float sumY = 0;

                for (int j = 0; j < i.orePerRegion; j++)
                {
                    float randX = randObj.NextFloat((float)randPos.x, (float)randPos.x + i.minSize);
                    float randY = randObj.NextFloat((float)randPos.y, (float)randPos.y + i.minSize);
                    var castPos = new Vector3(randX, castHeight, randY);
                    if (Physics.Raycast(castPos, Vector3.down, out hit, 500, mask))
                    {
                        sumY += hit.point.y;
                        if (hit.point.y < skipHeight) continue;
                        var randomAngle = randObj.NextFloat(0f, 360f);
                        var randomRotation = Quaternion.Euler(0, randomAngle, 0);
                        var tree = Instantiate(i.prefab[0], hit.point, randomRotation);
                        var scale = randObj.NextFloat(i.minScale, i.maxScale);
                        tree.transform.localScale = Vector3.one * scale;
                    }
                }
            }
        }
    }
    private Vector2Int GetRandomPos(CustomRandom randObj)
    {
        int randX = randObj.Next(0, 99);
        int randY = randObj.Next(0, 99);
        var coord = new Vector2Int(randX, randY);
        while (regionsOccupation.ContainsKey(coord))
        {
            randX = randObj.Next(0, 99);
            randY = randObj.Next(0, 99);
            coord = new Vector2Int(randX, randY);
        }
        regionsOccupation[coord] = true;
        return coord;
    }

    // Update is called once per frame
    void Update()
    {

    }
    [System.Serializable]
    class OreType
    {
        public string name;
        public List<GameObject> prefab;
        public float maxScale, minScale, minSize, maxSize;
        public float regionCount, orePerRegion;

    }
}
