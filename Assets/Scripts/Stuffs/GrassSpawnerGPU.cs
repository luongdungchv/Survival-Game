﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GrassSpawnerGPU : MonoBehaviour
{
    [SerializeField] private float grassCount, castHeight, testCull1, testCull2;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material grassMat;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private float culledDistance;
    [SerializeField] private int chunkPerEdge;

    private float chunkWidth;
    private int grassCountPerChunk;
    Dictionary<Vector2, GrassChunk> chunks;

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

        chunks = new Dictionary<Vector2, GrassChunk>();
        chunkWidth = 1500 / chunkPerEdge;
        var grassCountInt = Mathf.FloorToInt(1500 / grassCount);
        grassCountPerChunk = (int)Mathf.Pow(Mathf.FloorToInt(chunkWidth / grassCountInt), 2);
        for (int i = 0; i < chunkPerEdge; i++)
        {
            for (int j = 0; j < chunkPerEdge; j++)
            {
                var chunkPos = new Vector2(i * chunkWidth, j * chunkWidth);
                var chunk = new GrassChunk(Mathf.FloorToInt(chunkWidth), grassCountInt);
                chunk.lowerLeftPos = chunkPos;
                chunks.Add(chunkPos, chunk);
            }
        }

        transforms = new List<ShaderProps>();
        SpawnMatrix(waterHeight * maxHeight);
        InitCompute();
        Debug.Log(Camera.main.projectionMatrix.GetRow(0));


    }

    // Update is called once per frame
    void Update()
    {
        Draw();


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

                    var flooredX = Mathf.FloorToInt(x);
                    var flooredY = Mathf.FloorToInt(y);
                    var flooredChunkWidth = Mathf.FloorToInt(chunkWidth);
                    var chunkPos = new Vector2((flooredX / flooredChunkWidth) * flooredChunkWidth, (flooredY / flooredChunkWidth) * flooredChunkWidth);
                    //Debug.Log($"{chunkPos} {flooredX} {flooredY}");
                    var chosenChunk = chunks[chunkPos];


                    chosenChunk.AddProp(new ShaderProps()
                    {
                        pos = position,
                        trans = trs,
                        colorIndex = UnityEngine.Random.Range(0, 2),
                        normal = hitInfo.normal
                    });
                }
            }
        }

    }
    private void InitCompute()
    {

        transformArray = transforms.ToArray();
        culledArray = new ShaderProps[1500 * 1500];

        // shaderPropsBuffer = new ComputeBuffer(transformArray.Length, ShaderProps.Size());
        // shaderPropsBuffer.SetData(transformArray);

        culledBuffer = new ComputeBuffer(1500 * 1500, ShaderProps.Size(), ComputeBufferType.Append);
        culledBuffer.SetCounterValue(0);

        culledBuffer = new ComputeBuffer(1500 * 1500, ShaderProps.Size(), ComputeBufferType.Append);
        culledBuffer.SetCounterValue(0);


        argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)(grassCount * grassCount);
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);


        int kernelIndex = compute.FindKernel("CSMain");
        //        compute.SetBuffer(kernelIndex, "inputGrassBuffer", shaderPropsBuffer);
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

        meshBounds = new Bounds(transform.position, Vector3.one * (10000));
    }
    private void Draw()
    {
        Matrix4x4 P = Camera.main.projectionMatrix;
        P.SetRow(0, new Vector4(testCull1, 0, 0, 0));
        P.SetRow(1, new Vector4(0f, testCull2, 0, 0));
        Matrix4x4 V = Camera.main.worldToCameraMatrix;
        Matrix4x4 VP = P * V;

        var camPos = Camera.main.transform.position;
        int chunkCheckDistance = 35;
        float[] arr1 = { chunkCheckDistance, chunkCheckDistance, -chunkCheckDistance, -chunkCheckDistance };
        float[] arr2 = { chunkCheckDistance, -chunkCheckDistance, chunkCheckDistance, -chunkCheckDistance };
        List<GrassChunk> chunkToRender = new List<GrassChunk>();
        for (int i = 0; i < arr1.Length; i++)
        {
            var corner = new Vector2(camPos.x + arr1[i], camPos.z + arr2[i]);
            var flooredChunkWidth = Mathf.FloorToInt(chunkWidth);
            var chunkPos = new Vector2((Mathf.FloorToInt(corner.x) / flooredChunkWidth) * flooredChunkWidth, (Mathf.FloorToInt(corner.y) / flooredChunkWidth) * flooredChunkWidth);
            if (!chunkToRender.Contains(chunks[chunkPos]))
                chunkToRender.Add(chunks[chunkPos]);
        }


        var chosenData = new ShaderProps[grassCountPerChunk * chunkToRender.Count];
        for (int i = 0; i < chunkToRender.Count; i++)
        {
            Array.Copy(chunkToRender[i].props, 0, chosenData, i * grassCountPerChunk, grassCountPerChunk);
        }
        shaderPropsBuffer?.Release();
        shaderPropsBuffer = new ComputeBuffer(chosenData.Length, ShaderProps.Size());
        shaderPropsBuffer.SetData(chosenData);
        int kernelIndex = compute.FindKernel("CSMain");
        if (Input.GetKeyDown(KeyCode.X))
        {
            string res = "";
            foreach (var i in chunkToRender)
            {
                res += i.lowerLeftPos.ToString() + " ";
            }
            Debug.Log(res);
            Debug.Log(grassCountPerChunk);
        }
        compute.SetBuffer(kernelIndex, "inputGrassBuffer", shaderPropsBuffer);

        compute.SetMatrix("vp", VP);
        compute.SetVector("camPos", Camera.main.transform.position);
        compute.Dispatch(0, Mathf.CeilToInt(grassCount * grassCount / 64), 1, 1);

        var counterBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(culledBuffer, counterBuffer, 0);
        counterBuffer.GetData(args);
        var population = args[0];
        PopulateArgsBuffer(population);
        argsBuffer.SetData(args);



        counterBuffer.GetData(args);
        counterBuffer.Release();
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
    private void OnApplicationQuit()
    {
        argsBuffer.Release();
        shaderPropsBuffer.Release();
        culledBuffer.Release();
    }

}
public struct ShaderProps
{
    public Vector3 pos, normal;

    public Matrix4x4 trans;
    public int colorIndex;
    public static int Size()
    {
        return sizeof(float) * 22 + sizeof(int);
    }

}
public class GrassChunk
{
    public Vector2 lowerLeftPos;
    public ShaderProps[] props;
    private int counter;
    public GrassChunk(int chunkWidth, int grassDist)
    {
        var grassCount = Mathf.Pow(Mathf.FloorToInt((float)chunkWidth / grassDist), 2);
        props = new ShaderProps[(int)grassCount];
    }
    public void AddProp(ShaderProps prop)
    {
        props[counter] = prop;
        counter++;
    }

}
