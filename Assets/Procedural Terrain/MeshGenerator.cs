using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{

    public static Mesh GenerateMeshNoLOD(float[,] noiseMap, float maxHeight, AnimationCurve heightCurve, int worldWidth)
    {
        int width = noiseMap.GetLength(0);

        float unit = (float)worldWidth / (float)width;

        Vector3[] verts = new Vector3[width * width];
        Vector2[] uv = new Vector2[width * width];
        var tris = new List<int>();


        float tmpY = 0;
        float tmpX = 0;
        for (int y = 0; y < width; y++)
        {
            tmpX = 0;
            for (int x = 0; x < width; x++)
            {
                float noiseVal = noiseMap[x, y];
                var vertHeight = noiseVal * maxHeight * heightCurve.Evaluate(noiseMap[x, y]);
                if (vertHeight < Noise.minHeight) Noise.minHeight = vertHeight;
                if (vertHeight > Noise.maxHeight) Noise.maxHeight = vertHeight;
                var newVert = new Vector3(tmpX, vertHeight, tmpY);
                uv[y * width + x] = new Vector2((float)x / (float)width, (float)y / (float)width);
                verts[y * width + x] = newVert;
                tmpX += unit;
            }
            tmpY += unit;
        }
        for (int y = 0; y < width - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int lowLeft = width * y + x;
                int upLeft = lowLeft + width;
                int lowRight = lowLeft + 1;
                int upRight = upLeft + 1;
                tris.AddRange(new int[] { upRight, lowLeft, upLeft, lowRight, lowLeft, upRight });
            }
        }


        Mesh mesh = new Mesh();
        var trisArray = tris.ToArray();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = trisArray;
        mesh.RecalculateNormals();
        return mesh;
    }
    public static MeshData[] GenerateMeshes(float[,] noiseMap, float maxHeight, AnimationCurve heightCurve, int worldWidth)
    {
        int width = noiseMap.GetLength(0);

        float unit = (float)worldWidth / (float)width;

        Vector3[] verts = new Vector3[width * width];
        Vector2[] uv = new Vector2[width * width];
        var tris = new List<int>();


        float tmpY = 0;
        float tmpX = 0;
        for (int y = 0; y < width; y++)
        {
            tmpX = 0;
            for (int x = 0; x < width; x++)
            {
                float noiseVal = noiseMap[x, y];
                var vertHeight = noiseVal * maxHeight * heightCurve.Evaluate(noiseMap[x, y]);
                if (vertHeight < Noise.minHeight) Noise.minHeight = vertHeight;
                if (vertHeight > Noise.maxHeight) Noise.maxHeight = vertHeight;
                var newVert = new Vector3(tmpX, vertHeight, tmpY);
                uv[y * width + x] = new Vector2((float)x / (float)width, (float)y / (float)width);
                verts[y * width + x] = newVert;
                tmpX += unit;
            }
            tmpY += unit;
        }
        for (int y = 0; y < width - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int lowLeft = width * y + x;
                int upLeft = lowLeft + width;
                int lowRight = lowLeft + 1;
                int upRight = upLeft + 1;
                tris.AddRange(new int[] { upRight, lowLeft, upLeft, lowRight, lowLeft, upRight });
            }
        }
        Mesh[] meshs = new Mesh[2500];
        MeshData[] meshDatas = new MeshData[2500];
        int meshWidth = 50;
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int meshIndexX = x / 4;
                int meshIndexY = y / 4;
                int meshIndex = meshIndexY * meshWidth + meshIndexX;

                int xOdd = x % 4;
                int yOdd = y % 4;

                var selectedVert = verts[y * width + x];
                selectedVert = new Vector3(xOdd * unit, selectedVert.y, yOdd * unit);
                var selectedUV = uv[y * width + x];
            }
        }


        Mesh mesh = new Mesh();
        var trisArray = tris.ToArray();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = trisArray;
        mesh.RecalculateNormals();
        return meshDatas;
    }

    public static Mesh TestQuad()
    {
        Mesh res = new Mesh();

        var vertices = new Vector3[4];
        var uv = new Vector2[4];
        var triangles = new int[6];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0.5f, 0);
        uv[2] = new Vector2(0, 0.5f);
        uv[3] = new Vector2(.5f, .5f);

        vertices[0] = new Vector2(0, 0);
        vertices[1] = new Vector2(1, 0);
        vertices[2] = new Vector2(0, 1);
        vertices[3] = new Vector2(1, 1);

        triangles = new int[] { 1, 0, 3, 3, 0, 2 };

        res.vertices = vertices;
        res.uv = uv;
        res.triangles = triangles;

        return res;
    }
    [System.Serializable]
    public class MeshData
    {
        public Vector3 worldPos;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        private int vertCounter, trisCounter, uvCounter;
        public MeshData()
        {
            vertices = new Vector3[25];
            uv = new Vector2[25];
            triangles = new int[96];
        }
        public void AddVert(Vector3 newVert)
        {
            vertices[vertCounter] = newVert;
            vertCounter++;
        }
        public void AddTris(int newTris)
        {
            triangles[trisCounter] = newTris;
            trisCounter++;
        }
        public void AddUV(Vector2 newUV)
        {
            uv[uvCounter] = newUV;
            uvCounter++;
        }
    }

}

