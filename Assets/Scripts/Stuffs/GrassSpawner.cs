using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] grassPrefabs;
    [SerializeField] private float grassCount, castHeight;
    [SerializeField] private LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        var terrainTypes = GetComponent<MapGenerator>().terrainTypes;
        var maxHeight = GetComponent<MapGenerator>().vertMaxHeight;
        var waterHeight = terrainTypes[terrainTypes.Count - 2].height + 0.05f;
        SpawnGrass(waterHeight * maxHeight);

    }

    // Update is called once per frame
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
                    if (hitInfo.point.y < skipHeight) continue;
                    var hitNormal = hitInfo.normal;
                    var randomIndex = Random.Range(0, grassPrefabs.Length);
                    //Debug.Log(randomIndex);

                    var grassInstance = Instantiate(grassPrefabs[randomIndex], hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitNormal));
                    //var grassInstance = Instantiate(grassPrefabs[randomIndex], hitInfo.point, Quaternion.identity);

                    grassInstance.transform.position += new Vector3(0, Random.Range(-0.5f, 0.5f));
                    grassInstance.transform.Rotate(0, Random.Range(0, 180), 0);
                }
            }
        }
    }
}
