﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawnerGPU : MonoBehaviour
{
    [SerializeField] private float grassCount, castHeight, fovWidth, fovHeight;
    [SerializeField] private Vector2 grassScaleRange;
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
    private int chunkCheckDistance;
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

        Settings.OnSettingChange.AddListener(() =>
        {
            var settingJson = PlayerPrefs.GetString("setting", "");
            var settingData = new SettingData();
            if (settingJson != "")
            {
                settingData = JsonUtility.FromJson<SettingData>(settingJson);
            }
            int[] cullOptions = { 0, 40, 74 };
            int[] checkOptions = {0, 29, 46};
            this.culledDistance = cullOptions[settingData.lod];
            this.chunkCheckDistance = checkOptions[settingData.lod];
            compute.SetFloat("culledDist", culledDistance * culledDistance);
        });


        shaderPropsBuffer = new ComputeBuffer(transforms.Count, ShaderProps.Size());
        shaderPropsBuffer.SetData(this.transforms);

        int kernelIndex = compute.FindKernel("CSMain");
        compute.SetFloat("culledDist", culledDistance * culledDistance);
        compute.SetBuffer(kernelIndex, "inputGrassBuffer", shaderPropsBuffer);
        compute.SetBuffer(kernelIndex, "drawBuffer", argsBuffer);

    }
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
                    if (hitInfo.collider.tag == "Water") continue;
                    var position = hitInfo.point;
                    var rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                    var scale = Vector3.one * UnityEngine.Random.Range(grassScaleRange.x, grassScaleRange.y);
                    Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);

                    // var flooredX = Mathf.FloorToInt(x);
                    // var flooredY = Mathf.FloorToInt(y);
                    // var flooredChunkWidth = Mathf.FloorToInt(chunkWidth);
                    // var chunkPos = new Vector2((flooredX / flooredChunkWidth) * flooredChunkWidth, (flooredY / flooredChunkWidth) * flooredChunkWidth);
                    // var chosenChunk = chunks[chunkPos];
                    
                    var pos2D = new Vector2(position.x, position.z);
                    var chunkIndex = MapGenerator.ins.GetChunkIndex(pos2D);

                    transforms.Add(new ShaderProps()
                    {
                        pos = position,
                        chunkIndex = chunkIndex,
                        trans = trs,
                        normal = hitInfo.normal
                    });
                }
            }
        }

    }
    private void InitCompute()
    {
        culledArray = new ShaderProps[1500 * 1500];

        culledBuffer = new ComputeBuffer(1500 * 1500, ShaderProps.Size(), ComputeBufferType.Append);
        culledBuffer.SetCounterValue(0);

        culledBuffer = new ComputeBuffer(1500 * 1500, ShaderProps.Size(), ComputeBufferType.Append);
        culledBuffer.SetCounterValue(0);

        argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = 0;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer.SetData(args);

        int kernelIndex = compute.FindKernel("CSMain");
        compute.SetBuffer(kernelIndex, "culledGrassBuffer", culledBuffer);
        grassMat.SetBuffer("props", culledBuffer);

        meshBounds = new Bounds(transform.position, Vector3.one * (10000));
    }
    private void Draw()
    {
        Matrix4x4 P = Camera.main.projectionMatrix;
        P.SetRow(0, new Vector4(fovWidth, 0, 0, 0));
        P.SetRow(1, new Vector4(0f, fovHeight, 0, 0));
        Matrix4x4 V = Camera.main.worldToCameraMatrix;
        Matrix4x4 VP = P * V;

        compute.SetMatrix("vp", VP);
        compute.SetVector("camPos", Camera.main.transform.position);
        compute.SetVector("occupiedChunkIndices", MapGenerator.ins.GetOccupiedChunks(NetworkPlayer.localPlayer.transform.position, culledDistance));
            
        compute.Dispatch(0, Mathf.CeilToInt(transforms.Count / 64), 1, 1);

        ComputeBuffer.CopyCount(culledBuffer, argsBuffer, 4);

        Graphics.DrawMeshInstancedIndirect(mesh, 0, grassMat, meshBounds, argsBuffer);
        culledBuffer.SetCounterValue(0);


    }
    private void OnApplicationQuit()
    {
        argsBuffer?.Release();
        shaderPropsBuffer?.Release();
        culledBuffer?.Release();
    }

}
public struct ShaderProps
{
    public Vector3 pos, normal;

    public Matrix4x4 trans;
    public int chunkIndex;
    public static int Size()
    {
        return sizeof(float) * 22 + sizeof(int) * 1;
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
