using System.Collections.Generic;
using System.Net.Cache;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FlowerGPU : MonoBehaviour
{
    [SerializeField] private Color[] testColors;
    [SerializeField] private int noiseSize, texCount;
    [SerializeField] private float noiseScale, threshold, castHeight, culledDist;
    [SerializeField] private LayerMask mask;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material flowerMat, testMat;
    private List<InstanceData> datas;
    //private renderInstances
    private ComputeBuffer argsBuffer, instanceBuffer, renderBuffer;
    private Bounds bounds;
    private uint[] args;

    private void Start()
    {
        //SpawnFlower();
        bounds = new Bounds(transform.position, Vector3.one * (1500));
        GenerateInstanceData();
        InitBuffers();
        Draw();
    }
    void Update()
    {
        Draw();
    }

    public void InitBuffers()
    {
        var dataArray = datas.ToArray();
        instanceBuffer = new ComputeBuffer(datas.Count, InstanceData.size);
        instanceBuffer.SetData(dataArray);

        renderBuffer = new ComputeBuffer(datas.Count, InstanceData.size, ComputeBufferType.Append);
        renderBuffer.SetCounterValue(0);

        argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)(datas.Count);
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        compute.SetBuffer(0, "instanceBuffer", instanceBuffer);
        compute.SetBuffer(0, "renderBuffer", renderBuffer);
        compute.SetFloat("culledDist", culledDist);

        flowerMat.SetBuffer("instDatas", renderBuffer);

    }

    public void Draw()
    {
        Matrix4x4 P = Camera.main.projectionMatrix;
        P.SetRow(1, new Vector4(0.7f, 0, 0, 0));
        P.SetRow(1, new Vector4(0f, 1.2f, 0, 0));
        Matrix4x4 V = Camera.main.worldToCameraMatrix;
        Matrix4x4 VP = P * V;

        compute.SetMatrix("vp", VP);
        compute.SetVector("camPos", Camera.main.transform.position);
        compute.Dispatch(0, Mathf.CeilToInt(datas.Count / 64), 1, 1);

        var counterBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(renderBuffer, counterBuffer, 0);
        counterBuffer.GetData(args);
        var population = args[0];

        PopulateArgsBuffer(population);
        argsBuffer.SetData(args);
        counterBuffer.Release();
        Graphics.DrawMeshInstancedIndirect(mesh, 0, flowerMat, bounds, argsBuffer);
        renderBuffer.SetCounterValue(0);
        if (Input.GetKeyDown(KeyCode.L))
        {
            //Debug.Log(population);
            InstanceData[] arr = new InstanceData[1500];
            renderBuffer.GetData(arr);
            Debug.Log(arr[0].texIndex);
        }
    }

    private void GenerateInstanceData()
    {
        RaycastHit hit;
        var tex = new Texture2D(noiseSize, noiseSize);
        var noiseMap = Noise.GenerateNoiseDiscrete(noiseSize, noiseSize, noiseScale, -Vector2.zero, threshold);
        RandomizeMap(noiseMap);
        datas = new List<InstanceData>();
        for (int i = 0; i < noiseSize; i++)
        {
            for (int j = 0; j < noiseSize; j++)
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

                        var position = hit.point + hit.normal * Random.Range(0.5f, 1f);
                        var trs = Matrix4x4.TRS(position, Quaternion.Euler(-90, 0, 0), Vector3.one * 0.1f);
                        var texIndex = (int)noiseVal - 1;
                        datas.Add(new InstanceData()
                        {
                            position = position,
                            trs = trs,
                            texIndex = texIndex
                        });
                    }
                }
            }
        }
        tex.Apply();
        testMat.mainTexture = tex;

    }
    private void PopulateArgsBuffer(uint population)
    {
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)(population);
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
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
                    if (connectedVal == 0) connectedVal = randObj.Next(1, texCount + 1);
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
    struct InstanceData
    {
        public Vector3 position;
        public Matrix4x4 trs;
        public int texIndex;
        public static int size => sizeof(float) * 19 + sizeof(int);
    }
}