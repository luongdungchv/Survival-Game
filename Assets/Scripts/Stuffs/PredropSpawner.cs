using System.Collections.Generic;
using System.Net.Cache;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.Analytics;

public class PredropSpawner : MonoBehaviour
{
    [SerializeField] PredropType[] types;
    [SerializeField] private int size, castHeight;
    [SerializeField] private LayerMask mask;

    private CustomRandom randObj;

    private void Start()
    {
        randObj = new CustomRandom(MapGenerator.ins.seed);
        SpawnPredrop();

    }

    private void SpawnPredrop()
    {
        foreach (var i in types)
        {
            var spawnPositions = RandomizeMap(i.count);

            RaycastHit hitInfo;
            int count = 0;
            foreach (var pos in spawnPositions)
            {
                var castPos = new Vector3(pos.x, castHeight, pos.y);
                if (Physics.Raycast(castPos, Vector3.down, out hitInfo, castHeight + 50, mask))
                {
                    if (hitInfo.collider.tag == "Water") continue;
                    count++;
                    var rotateToSlope = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                    var instance = Instantiate(i.prefab, hitInfo.point, rotateToSlope);
                    instance.transform.Rotate(0, Random.Range(0, 360), 0);

                    var drop = instance.GetComponentInChildren<ItemDrop>();
                    drop.SetQuantity(1);
                    drop.SetBase(Item.GetItem(i.itemName));
                }
            }
        }
    }
    private HashSet<Vector2Int> RandomizeMap(int count)
    {
        int i = 0;
        HashSet<Vector2Int> occupation = new HashSet<Vector2Int>();
        RaycastHit hitInfo;

        while (i < count)
        {
            int x = randObj.Next(0, size);
            int y = randObj.Next(0, size);
            var coord = new Vector2Int(x, y);

            var castPos = new Vector3(x, castHeight, y);
            if (!occupation.Contains(coord) && Physics.Raycast(castPos, Vector3.down, out hitInfo, castHeight + 50, mask))
            {
                if (hitInfo.collider.tag == "Water") continue;
                occupation.Add(coord);
                i++;
            }
        }
        return occupation;
    }
}
[System.Serializable]
public class PredropType
{
    public string name;
    public string itemName;
    public GameObject prefab;
    public int count;
}