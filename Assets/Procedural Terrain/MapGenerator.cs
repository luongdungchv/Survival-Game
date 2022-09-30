using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator ins;
    public int seed;
    public int width, texSize;
    public float scale;
    public int octaves;
    public float lacunarity;
    public float persistence;
    public Vector2 offset;
    public AnimationCurve heightCurve;
    public float vertMaxHeight;
    public int lod;
    public float falloff;
    public List<TerrainType> terrainTypes;
    public Material terrainMat;
    [Range(0, 1)]
    public List<float> blends;
    // Start is called before the first frame update
    void Start()
    {
        ins = this;
        var mesh = GetComponent<MeshFilter>().mesh;
        var verts = mesh.vertices;

        UpdateMesh();
        var rand = new CustomRandom(MapGenerator.ins.seed);

    }

    // Update is called once per frame

    public void UpdateTexture(float[,] noiseMap)
    {


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
        float[,] noiseMap = Noise.GenerateNoiseBase(width, width, scale, octaves, lacunarity, persistence, offsetVector, falloff);
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
}
[System.Serializable]
public struct TerrainType
{
    public string label;
    public float height;
    public Color color;
    public Texture2D texture;
}
