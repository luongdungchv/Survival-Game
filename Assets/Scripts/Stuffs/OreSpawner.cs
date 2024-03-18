using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OreSpawner : MonoBehaviour
{
    public int cellDist;
    [SerializeField] private List<OreType> oreTypes;
    [SerializeField] private float castHeight;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Image hpBarUIPrefab;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject hpBarUIContainer;

    private Dictionary<Vector2Int, bool> regionsOccupation;
    private int seed;
    private int mithrilCount;
    public void Init()
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
                if (regionsOccupation.Count >= 10000) break;
                var randPos = GetRandomPos(randObj);

                randPos = new Vector2Int(randPos.x * cellDist, randPos.y * cellDist);

                float sumY = 0;

                for (int j = 0; j < i.orePerRegion; j++)
                {
                    float randSize = randObj.NextFloat(i.minSize, i.maxSize);
                    float randX = randObj.NextFloat((float)randPos.x, (float)randPos.x + randSize);
                    float randY = randObj.NextFloat((float)randPos.y, (float)randPos.y + randSize);
                    var castPos = new Vector3(randX, castHeight, randY);
                    if (Physics.Raycast(castPos, Vector3.down, out hit, 500, mask))
                    {


                        sumY += hit.point.y;
                        if (hit.collider.tag == "Water") continue;
                        if(i.name == "Mithril Ore") mithrilCount++;
                        var randomAngle = randObj.NextFloat(0f, 360f);
                        var rotateToSlope = Quaternion.FromToRotation(Vector3.up, hit.normal);

                        var randomPrefab = i.prefab[randObj.Next(0, i.prefab.Count)];
                        var ore = Instantiate(randomPrefab, hit.point, rotateToSlope);

                        var scale = randObj.NextFloat(i.minScale, i.maxScale);
                        ore.transform.localScale = Vector3.one * scale;

                        ore.transform.Rotate(0, randomAngle, 0);

                        MapGenerator.ins.AddObjToStaticBatching(ore.transform);

                    }
                }
            }
        }
        Debug.Log(mithrilCount);
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
        public float minScale, maxScale, minSize, maxSize;
        public float regionCount, orePerRegion;

    }
}
