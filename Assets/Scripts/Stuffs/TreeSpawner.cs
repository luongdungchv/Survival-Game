using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note: Total number of region is 100 (max width of 150 each region)
public class TreeSpawner : MonoBehaviour
{
    const int CELL_DIST = 150;
    [SerializeField] private List<Region> regions;
    [SerializeField] private float castHeight;
    [SerializeField] private LayerMask mask;
    //[SerializeField] private GameObject cherryPetalParticle;
    private Dictionary<Vector2Int, bool> regionsOccupation;
    private int seed;
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
        foreach (var i in regions)
        {
            //Spawn Tree
            for (int a = 0; a < i.regionCount; a++)
            {
                if (regionsOccupation.Count >= 100) break;
                var randPos = GetRandomPos(randObj);

                randPos = new Vector2Int(randPos.x * CELL_DIST, randPos.y * CELL_DIST);

                float sumY = 0;

                for (int j = 0; j < i.treesPerRegion; j++)
                {
                    float randX = randObj.NextFloat((float)randPos.x, (float)randPos.x + CELL_DIST);
                    float randY = randObj.NextFloat((float)randPos.y, (float)randPos.y + CELL_DIST);
                    var castPos = new Vector3(randX, castHeight, randY);
                    if (Physics.Raycast(castPos, Vector3.down, out hit, 500, mask))
                    {
                        sumY += hit.point.y;
                        if (hit.point.y < skipHeight) continue;
                        var randomAngle = randObj.NextFloat(0f, 360f);
                        var randomRotation = Quaternion.Euler(0, randomAngle, 0);
                        var tree = Instantiate(i.prefab, hit.point, randomRotation);
                        var scale = randObj.NextFloat(i.minScale, i.maxScale);
                        tree.transform.localScale = Vector3.one * scale;
                    }
                }
                sumY /= i.treesPerRegion;
                sumY += 25;

                Vector2 centerPos = new Vector2(randPos.x + CELL_DIST / 2, randPos.y + CELL_DIST / 2);
                Vector3 petalSpawnPos = new Vector3(centerPos.x, sumY, centerPos.y);
                Vector3 randRotationTo = new Vector3(randObj.NextFloat(-1, 1), -1, randObj.NextFloat(-1, 1));
                var petalRotation = Quaternion.FromToRotation(Vector3.forward, randRotationTo);
                var petal = Instantiate(i.petalParticlePrefab, petalSpawnPos, petalRotation);
            }
        }
    }
    private Vector2Int GetRandomPos(CustomRandom randObj)
    {
        int randX = randObj.Next(0, 9);
        int randY = randObj.Next(0, 9);
        var coord = new Vector2Int(randX, randY);
        while (regionsOccupation.ContainsKey(coord))
        {
            randX = randObj.Next(0, 9);
            randY = randObj.Next(0, 9);
            coord = new Vector2Int(randX, randY);
        }
        regionsOccupation[coord] = true;
        return coord;
    }

    void Update()
    {

    }
}
[System.Serializable]
public class Region
{
    public string name;
    public GameObject prefab, petalParticlePrefab;
    public float maxWidth, minWidth, regionCount, treesPerRegion, minScale, maxScale;


}
