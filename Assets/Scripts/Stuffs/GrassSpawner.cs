using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] grassPrefabs;
    [SerializeField] private float grassCount, castHeight;
    [SerializeField] private LayerMask mask;
    void Start()
    {
        var terrainTypes = GetComponent<MapGenerator>().terrainTypes;
        var maxHeight = GetComponent<MapGenerator>().vertMaxHeight;
        var waterHeight = terrainTypes[terrainTypes.Count - 2].height + 0.05f;
        SpawnGrass(waterHeight * maxHeight);

    }

    void Update()
    {

    }
    private void SpawnGrass(float skipHeight)
    {
        float grassDistance = 1500 / grassCount;
        Vector3 startPos = transform.position;
        for (float x = 0; x <= 1500; x += grassDistance)
        {
            for (float y = 0; y <= 1500; y += grassDistance)
            {
                var castPos = new Vector3(x, castHeight, y);
                RaycastHit hitInfo;
                if (Physics.Raycast(castPos, Vector3.down, out hitInfo, castHeight + 500, mask))
                {
                    if (hitInfo.point.y < skipHeight || hitInfo.collider.tag == "Water") continue;
                    var hitNormal = hitInfo.normal;
                    var randomIndex = Random.Range(0, grassPrefabs.Length);

                    var grassInstance = Instantiate(grassPrefabs[randomIndex], hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitNormal));

                    grassInstance.transform.position += new Vector3(0, Random.Range(-0.5f, 0.5f));
                    grassInstance.transform.Rotate(0, Random.Range(0, 180), 0);
                }
            }
        }
    }
}