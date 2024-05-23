using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class InstancedDrawer : MonoBehaviour
{
    private Dictionary<Material, MaterialBufferData> materialInfos;
    private Dictionary<Mesh, MeshInfoData> meshInfos;

    private Bounds meshBounds;

    private void Awake()
    {
        meshBounds = new Bounds(transform.position, Vector3.one * (10000));
    }

    private void SetupBuffer()
    {
        var materialTempData = new Dictionary<Material, MaterialData>();
        foreach (var mesh in meshInfos.Keys)
        {
            var data = meshInfos[mesh];
            data.argsOffsetList = new List<int>[data.materialList.Count];
            for (int j = 0; j < data.lodMeshes.Count; j++)
            {
                for (int i = 0; i < data.materialList.Count; i++)
                {
                    var mat = data.materialList[i];
                    if (materialTempData.ContainsKey(mat))
                    {
                        uint[] args = {
                            data.lodMeshes[j].GetIndexCount(i),
                            (uint)data.instanceDataList.Count,
                            (uint)materialTempData[mat].indexList.Count + data.lodMeshes[j].GetIndexStart(i),
                            (uint)materialTempData[mat].vertexList.Count + data.lodMeshes[j].GetBaseVertex(i),
                            (uint)materialTempData[mat].instanceDataList.Count
                        };
                        materialTempData[mat].instanceDataList.AddRange(data.instanceDataList);
                        var submesh = data.lodMeshes[j].GetSubMesh(i);
                        var vertices = data.lodMeshes[j].vertices;
                        for (int x = submesh.baseVertex; x < submesh.vertexCount; x++)
                        {
                            materialTempData[mat].vertexList.Add(vertices[x]);
                        }
                        materialTempData[mat].indexList.AddRange(data.lodMeshes[j].GetIndices(i));
                        materialTempData[mat].argList.AddRange(args);
                    }
                }
            }

        }

        this.materialInfos ??= new Dictionary<Material, MaterialBufferData>();
        this.materialInfos.Clear();
        foreach (var key in materialTempData.Keys)
        {
            var bufferData = new MaterialBufferData();
            var tempInfo = materialTempData[key];

            bufferData.argsBuffer = new ComputeBuffer(tempInfo.argList.Count, sizeof(int), ComputeBufferType.IndirectArguments);
            bufferData.argsBuffer.SetData(tempInfo.argList);

            bufferData.vertexBuffer = new ComputeBuffer(tempInfo.vertexList.Count, sizeof(float) * 3);
            bufferData.vertexBuffer.SetData(tempInfo.vertexList);

            bufferData.indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, tempInfo.indexList.Count, sizeof(int));
            bufferData.indexBuffer.SetData(tempInfo.indexList);

            bufferData.instanceBuffer = new ComputeBuffer(tempInfo.instanceDataList.Count, InstanceData.Size);
            bufferData.instanceBuffer.SetData(tempInfo.instanceDataList);

            this.materialInfos.Add(key, bufferData);

            key.SetBuffer("instanceBuffer", bufferData.instanceBuffer);
            key.SetBuffer("vertexBuffer", bufferData.vertexBuffer);
        }
    }

    public void Draw()
    {
        foreach (var mesh in this.meshInfos.Keys)
        {
            var meshInfo = meshInfos[mesh];

            foreach (var material in meshInfo.materialList)
            {
                var materialInfo = materialInfos[material];
                for (int i = 0; i < meshInfo.lodMeshes.Count; i++)
                {
                   // Graphics.DrawProceduralIndirect(material, meshBounds, MeshTopology.Triangles, materialInfo.indexBuffer, meshInfo.argsOffsetList[i]);
                }
            }
        }
    }

    public struct InstanceData
    {
        public Matrix4x4 trs;
        public int lodLevel;
        public static int Size => sizeof(float) * 16 + sizeof(int);
    }
    public class MaterialBufferData
    {
        public ComputeBuffer instanceBuffer, argsBuffer, vertexBuffer;
        public GraphicsBuffer indexBuffer;
    }
    public class MaterialData
    {
        public List<InstanceData> instanceDataList;
        public List<uint> argList;
        public List<Vector3> vertexList;
        public List<int> indexList;
        public MaterialData()
        {
            instanceDataList = new List<InstanceData>();
            argList = new List<uint>();
            vertexList = new List<Vector3>();
            indexList = new List<int>();
        }

    }
    public class MeshInfoData
    {
        public List<Mesh> lodMeshes;
        public List<InstanceData> instanceDataList;
        public List<Material> materialList;
        public List<int>[] argsOffsetList;

    }
}

