using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BushesSpawner : MonoBehaviour
{
    [SerializeField] private BushType[] bushTypes;
    [SerializeField] private int seedOffset, countPerEdge;
    [SerializeField] private float castHeight;
    [SerializeField] private LayerMask mask;
    private CustomRandom randObj;
    private HashSet<Vector2Int> regionOccupation;
    // Start is called before the first frame update
    void Start()
    {
        regionOccupation = new HashSet<Vector2Int>();
        randObj = new CustomRandom(MapGenerator.ins.seed + seedOffset);
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Spawn()
    {
        int regionGap = 1500 / countPerEdge;
        foreach (var type in bushTypes)
        {
            for (int i = 0; i < type.regionCount; i++)
            {
                var regionPos = GenerateRegion();
                Debug.Log(regionPos);
                for (int j = 0; j < type.countPerRegion; j++)
                {
                    var posX = randObj.NextFloat(regionPos.x * regionGap, regionPos.x * regionGap + regionGap);
                    var posY = randObj.NextFloat(regionPos.y * regionGap, regionPos.y * regionGap + regionGap);
                    var spawnState = SpawnBush(type, posX, posY);
                }
            }
        }
    }
    private Vector2Int GenerateRegion()
    {

        int randX = randObj.Next(0, countPerEdge);
        int randY = randObj.Next(0, countPerEdge);
        var coord = new Vector2Int(randX, randY);
        while (regionOccupation.Contains(coord))
        {
            randX = randObj.Next(0, countPerEdge);
            randY = randObj.Next(0, countPerEdge);
            coord = new Vector2Int(randX, randY);
        }
        regionOccupation.Add(coord);

        return coord;
    }
    private bool SpawnBush(BushType type, float posX, float posY)
    {
        var castPos = new Vector3(posX, castHeight, posY);
        RaycastHit hitInfo;
        if (Physics.Raycast(castPos, Vector3.down, out hitInfo, 100, mask))
        {
            if (hitInfo.collider.tag == "Water") return false;
            var rotationToSlope = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            var obj = Instantiate(type.prefab, hitInfo.point, rotationToSlope);
            var randomScaleFactor = randObj.NextFloat(type.minScale, type.maxScale);
            obj.transform.localScale = Vector3.one * randomScaleFactor;
            obj.transform.Rotate(0, randObj.NextFloat(0, 360), 0);
        }
        return true;
    }
}
[System.Serializable]
public class BushType
{
    public string name;
    public GameObject prefab;
    public float minScale;
    public float maxScale;
    public int regionCount;
    public int countPerRegion;
}
