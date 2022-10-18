using System.Collections.Generic;
using System.Net.Cache;
using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private GameObject[] flowerPrefab;
    [SerializeField] private Color[] testColors;
    [SerializeField] private int size;
    [SerializeField] private float noiseScale, threshold, castHeight;
    [SerializeField] private LayerMask mask;
    public Material testmat;

    private void Start()
    {
        SpawnFlower();
    }

    private void SpawnFlower()
    {
        RaycastHit hit;
        var tex = new Texture2D(size, size);
        var noiseMap = Noise.GenerateNoiseDiscrete(size, size, noiseScale, -Vector2.zero, threshold);
        RandomizeMap(noiseMap);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float noiseVal = noiseMap[i, j];
                // tex.SetPixel(i, j, noiseVal > threshold ? Color.white : Color.black);
                tex.SetPixel(i, j, testColors[(int)noiseVal]);
                if (noiseVal > threshold)
                {
                    var castPos = new Vector3(i, castHeight, j);
                    castPos += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.4f, 0.4f));
                    if (Physics.Raycast(castPos, Vector3.down, out hit, castHeight * 2, mask))
                    {
                        if (hit.collider.tag == "Water") continue;
                        var flower = Instantiate(flowerPrefab[(int)noiseVal - 1], hit.point, Quaternion.Euler(-90, 0, 0));
                        flower.transform.position += Vector3.up * Random.Range(0.3f, 0.7f);

                        var randomRot = Random.Range(0, 70);
                        flower.transform.Rotate(0, 0, randomRot);

                        var randomScale = Random.Range(1, 2.5f);
                        flower.transform.localScale /= randomScale;
                    }
                }
            }
        }
        tex.Apply();
        testmat.mainTexture = tex;

    }
    private void RandomizeMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        var randObj = new CustomRandom(MapGenerator.ins.seed);
        for (int x = 0; x < width; x++)
        {
            List<Vector2Int> scannedpos = new List<Vector2Int>();
            float connectedVal = 0;
            for (int y = 0; y < height; y++)
            {
                float noiseVal = noiseMap[x, y];
                //bool isConnected = false;
                if (noiseVal > 0)
                {
                    if (x > 0 && noiseMap[x - 1, y] > 0)
                    {
                        connectedVal = noiseMap[x - 1, y];
                    }
                    scannedpos.Add(new Vector2Int(x, y));
                }
                else
                {
                    //Debug.Log("t");
                    //if (scannedpos.Count > 0) Debug.Log(scannedpos.Count);
                    if (connectedVal == 0) connectedVal = randObj.Next(1, flowerPrefab.Length + 1);
                    foreach (var i in scannedpos)
                    {
                        noiseMap[i.x, i.y] = connectedVal;
                    }
                    connectedVal = 0;
                    scannedpos = new List<Vector2Int>();
                }
            }
        }
    }
}