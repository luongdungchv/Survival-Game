using System.Collections.Generic;
using System.Net.Cache;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FlowerGPU : MonoBehaviour
{
    [SerializeField] private Color[] testColors;
    [SerializeField] private int noiseSize, texCount;
    [SerializeField] private float noiseScale, threshold, castHeight, culledDist, chunkPerEdge;
    [SerializeField] private LayerMask mask;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material flowerMat, testMat;
    private Dictionary<Vector2, FlowerChunk> chunks;
    private List<InstanceData> datas;
    private int chunkWidth;
    private int instancePerChunk;
    //private renderInstances
    private ComputeBuffer argsBuffer, instanceBuffer, renderBuffer;
    private Bounds bounds;
    private uint[] args;
    private int instanceCount;

    private void Start()
    {
        //SpawnFlower();
        bounds = new Bounds(transform.position, Vector3.one * (1500));
        chunkWidth = Mathf.FloorToInt(1500 / chunkPerEdge);
        instancePerChunk = chunkWidth * chunkWidth;
        chunks = new Dictionary<Vector2, FlowerChunk>();
        for (int i = 0; i < chunkPerEdge; i++)
        {
            for (int j = 0; j < chunkPerEdge; j++)
            {
                var chunkPos = new Vector2(i * chunkWidth, j * chunkWidth);
                var chunk = new FlowerChunk();
                chunk.lowerLeftPos = chunkPos;
                chunks.Add(chunkPos, chunk);
            }
        }
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
        // var dataArray = datas.ToArray();
        // instanceBuffer = new ComputeBuffer(datas.Count, InstanceData.size);
        // instanceBuffer.SetData(dataArray);

        renderBuffer = new ComputeBuffer(instanceCount, InstanceData.size, ComputeBufferType.Append);
        renderBuffer.SetCounterValue(0);

        argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)(datas.Count);
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        //compute.SetBuffer(0, "instanceBuffer", instanceBuffer);
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

        var camPos = Camera.main.transform.position;
        int chunkCheckDistance = 35;
        float[] arr1 = { chunkCheckDistance, chunkCheckDistance, -chunkCheckDistance, -chunkCheckDistance };
        float[] arr2 = { chunkCheckDistance, -chunkCheckDistance, chunkCheckDistance, -chunkCheckDistance };
        List<FlowerChunk> chunkToRender = new List<FlowerChunk>();
        for (int i = 0; i < arr1.Length; i++)
        {
            var corner = new Vector2(camPos.x + arr1[i], camPos.z + arr2[i]);
            var flooredChunkWidth = Mathf.FloorToInt(chunkWidth);
            var chunkPos = new Vector2((Mathf.FloorToInt(corner.x) / chunkWidth) * chunkWidth, (Mathf.FloorToInt(corner.y) / chunkWidth) * chunkWidth);
            if (!chunkToRender.Contains(chunks[chunkPos]))
                chunkToRender.Add(chunks[chunkPos]);
        }

        var chosenData = new List<InstanceData>();
        for (int i = 0; i < chunkToRender.Count; i++)
        {
            //Array.Copy(chunkToRender[i].props, 0, chosenData, i * instancePerChunk, instancePerChunk);
            chosenData.AddRange(chunkToRender[i].props);
        }
        instanceBuffer?.Release();
        instanceBuffer = new ComputeBuffer(chosenData.Count, InstanceData.size);
        instanceBuffer.SetData(chosenData);
        int kernelIndex = compute.FindKernel("CSMain");
        compute.SetBuffer(kernelIndex, "instanceBuffer", instanceBuffer);

        compute.SetMatrix("vp", VP);
        compute.SetVector("camPos", Camera.main.transform.position);
        compute.Dispatch(0, Mathf.CeilToInt(chosenData.Count / 64), 1, 1);

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

                        var flooredX = Mathf.FloorToInt(i);
                        var flooredY = Mathf.FloorToInt(j);
                        var chunkPos = new Vector2((flooredX / chunkWidth) * chunkWidth, (flooredY / chunkWidth) * chunkWidth);
                        //Debug.Log(chunkPos);
                        var chosenChunk = chunks[chunkPos];

                        chosenChunk.AddProp(new InstanceData()
                        {
                            position = position,
                            trs = trs,
                            texIndex = texIndex
                        });
                        instanceCount++;
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

    private void OnApplicationQuit()
    {
        instanceBuffer.Release();
        renderBuffer.Release();
        argsBuffer.Release();
    }
}
public struct InstanceData
{
    public Vector3 position;
    public Matrix4x4 trs;
    public int texIndex;
    public static int size => sizeof(float) * 19 + sizeof(int);
}
public class FlowerChunk
{
    public Vector2 lowerLeftPos;
    public List<InstanceData> props;
    private int counter;
    public FlowerChunk()
    {
        props = new List<InstanceData>();
    }
    public void AddProp(InstanceData prop)
    {
        props.Add(prop);
    }
}