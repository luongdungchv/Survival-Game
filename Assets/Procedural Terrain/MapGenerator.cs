using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator ins;
    public int seed;
    [SerializeField] private int width, texSize;
    [SerializeField] private float scale, falloff;
    public float vertMaxHeight;

    public AnimationCurve heightCurve;
    public Vector2 offset;
    [SerializeField] private Material mapMat, terrainMat;
    [Range(0, 1), SerializeField]
    private List<float> blends;
    public List<TerrainType> terrainTypes;
    private float[,] noiseMap;
    // Start is called before the first frame update
    void Awake()
    {
        ins = this;
        seed = Client.ins.mapSeed;
        var mesh = GetComponent<MeshFilter>().mesh;
        var verts = mesh.vertices;

        UpdateMesh();
        var rand = new CustomRandom(MapGenerator.ins.seed);

    }

    // Update is called once per frame

    public void UpdateTexture(float[,] noiseMap)
    {

        int width = noiseMap.GetLength(0);
        int length = noiseMap.GetLength(1);
        Texture2D mapColor = new Texture2D(width, length);
        for (int i = 0; i < noiseMap.GetLength(0); i++)
        {
            for (int j = 0; j < noiseMap.GetLength(1); j++)
            {
                float noiseVal = noiseMap[i, j];
                if (noiseVal * heightCurve.Evaluate(noiseVal) <= 0.1125f) mapColor.SetPixel(i, j, Color.cyan);
                else mapColor.SetPixel(i, j, Color.green);

            }
        }
        mapColor.Apply();
        mapMat.mainTexture = mapColor;
        var baseColors = terrainTypes.Select(n => n.color).ToArray();
        var baseHeights = terrainTypes.Select(n => n.height).ToArray();

        terrainMat.SetInt("baseColorCount", terrainTypes.Count);
        terrainMat.SetColorArray("baseColors", baseColors);
        terrainMat.SetFloatArray("baseHeights", baseHeights);
        terrainMat.SetFloatArray("baseBlends", blends);
        terrainMat.SetFloat("minHeight", 0);
        terrainMat.SetFloat("maxHeight", vertMaxHeight);
        terrainMat.SetFloat("_testScale", 9);

        var texArray = CreateTextureArray();
        terrainMat.SetTexture("baseTextures", texArray);

    }
    public void UpdateMesh()
    {
        var randObj = new CustomRandom(seed);
        var offsetVector = new Vector2(randObj.NextFloat(0, 1000), randObj.NextFloat(0, 1000));
        noiseMap = Noise.GenerateNoiseBase(width, width, scale, offsetVector, falloff);
        var mesh = MeshGenerator.GenerateMeshNoLOD(noiseMap, vertMaxHeight, heightCurve, 1500);
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateTexture(noiseMap);
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    public Task MeshUpdate()
    {
        UpdateMesh();
        return null;
    }

    private int Compare(TerrainType x, TerrainType y)
    {
        if (x.height > y.height) return 1;
        if (x.height < y.height) return -1;
        return 0;
    }
    private Texture2DArray CreateTextureArray()
    {
        Texture2DArray texArray = new Texture2DArray(texSize, texSize, terrainTypes.Count, TextureFormat.RGB565, true);
        for (int i = 0; i < terrainTypes.Count; i++)
        {
            var type = terrainTypes[i];
            var pixels = type.texture.GetPixels();
            texArray.SetPixels(type.texture.GetPixels(), i);
        }
        texArray.Apply();

        return texArray;
    }
    public bool BelowWater(int x, int y)
    {
        Debug.Log(noiseMap.GetLength(0));
        var noiseVal = noiseMap[x, y];
        return noiseVal * vertMaxHeight * heightCurve.Evaluate(noiseVal) < 9;
    }
}
[System.Serializable]
public struct TerrainType
{
    public string label;
    public float height;
    public Color color;
    public Texture2D texture;
}
