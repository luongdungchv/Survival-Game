using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] grassPrefabs;
    [SerializeField] private float grassCount, castHeight;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material grassMat;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private float culledDistance;

    private ComputeBuffer argsBuffer, shaderPropsBuffer, culledBuffer;
    private Bounds meshBounds;
    private List<ShaderProps> transforms;
    private ShaderProps[] transformArray;
    private ShaderProps[] culledArray;
    private uint[] args;
    public Transform testpos;
    // Start is called before the first frame update
    void Start()
    {
        var terrainTypes = GetComponent<MapGenerator>().terrainTypes;
        var maxHeight = GetComponent<MapGenerator>().vertMaxHeight;
        var waterHeight = terrainTypes[terrainTypes.Count - 2].height + 0.05f;

        transforms = new List<ShaderProps>();
        SpawnMatrix(waterHeight * maxHeight);
        InitCompute();


    }

    // Update is called once per frame
    void Update()
    {
        Draw();


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
    private void SpawnMatrix(float skipHeight)
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
                    var position = hitInfo.point;
                    var rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                    var scale = Vector3.one;
                    Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);
                    transforms.Add(new ShaderProps() { pos = position, trans = trs, colorIndex = Random.Range(0, 2) });
                }
            }
        }

    }
    private void InitCompute()
    {

        transformArray = transforms.ToArray();
        culledArray = new ShaderProps[transformArray.Length];

        shaderPropsBuffer = new ComputeBuffer(transformArray.Length, ShaderProps.Size());
        shaderPropsBuffer.SetData(transformArray);

        culledBuffer = new ComputeBuffer(transformArray.Length, ShaderProps.Size(), ComputeBufferType.Append);
        culledBuffer.SetCounterValue(0);

        culledBuffer = new ComputeBuffer(transformArray.Length, ShaderProps.Size(), ComputeBufferType.Append);
        culledBuffer.SetCounterValue(0);


        argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)(grassCount * grassCount);
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);


        int kernelIndex = compute.FindKernel("CSMain");
        compute.SetBuffer(kernelIndex, "inputGrassBuffer", shaderPropsBuffer);
        compute.SetBuffer(kernelIndex, "culledGrassBuffer", culledBuffer);
        compute.SetFloat("culledDist", culledDistance);
        // compute.Dispatch(0, Mathf.CeilToInt(grassCount * grassCount / 64), 1, 1);
        // ComputeBuffer.CopyCount(culledBuffer, argsBuffer, -2);
        // argsBuffer.GetData(args);
        // foreach (var i in args)
        // {
        //     Debug.Log(i);
        // }

        grassMat.SetBuffer("props", culledBuffer);

        meshBounds = new Bounds(transform.position, Vector3.one * (grassCount * 2 + 1));
    }
    private void Draw()
    {
        Matrix4x4 P = Camera.main.projectionMatrix;
        P.SetRow(1, new Vector4(0f, 1.2f, 0, 0));
        Matrix4x4 V = Camera.main.worldToCameraMatrix;
        Matrix4x4 VP = P * V;


        compute.SetMatrix("vp", VP);
        compute.SetVector("camPos", Camera.main.transform.position);
        compute.Dispatch(0, Mathf.CeilToInt(grassCount * grassCount / 64), 1, 1);

        var counterBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(culledBuffer, counterBuffer, 0);
        counterBuffer.GetData(args);
        var population = args[0];
        PopulateArgsBuffer(population);
        argsBuffer.SetData(args);

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(population);
        }

        counterBuffer.GetData(args);
        population = args[0];
        PopulateArgsBuffer(population);

        Graphics.DrawMeshInstancedIndirect(mesh, 0, grassMat, meshBounds, argsBuffer);
        //Graphics.DrawMeshInstancedIndirect(lowLodMesh, 0, grassMatLowLod, meshBounds, lowLodArgsBuffer);

        culledBuffer.SetCounterValue(0);

    }
    private void PopulateArgsBuffer(uint population)
    {
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)(population);
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

    }

}
public struct ShaderProps
{
    public Vector3 pos;
    public Matrix4x4 trans;
    public int colorIndex;
    public static int Size()
    {
        return sizeof(float) * 19 + sizeof(int);
    }
}
