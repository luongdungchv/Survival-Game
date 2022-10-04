using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public static Mesh GenerateMesh(float[,] noiseMap, float maxHeight, AnimationCurve heightCurve, int lod)
    {
        var width = noiseMap.GetLength(0);
        var height = noiseMap.GetLength(1);

        var lodWidth = (width - 1) / lod + 1;
        var lodHeight = (height - 1) / lod + 1;
        // var width = 50;
        // var height = 50;
        Mesh mesh = new Mesh();

        var verts = new Vector3[lodWidth * lodHeight];
        var uv = new Vector2[lodWidth * lodHeight];
        var normals = new Vector2[lodWidth * lodHeight];
        var tris = new List<int>();

        int vertIndex = 0;
        for (int y = 0; y < height; y += lod)
        {
            for (int x = 0; x < width; x += lod)
            {
                float vertHeight = noiseMap[x, y] * maxHeight * heightCurve.Evaluate(noiseMap[x, y]);
                verts[vertIndex] = new Vector3(x, vertHeight, y);
                uv[vertIndex] = new Vector2((float)x / width, (float)y / height);
                vertIndex++;
            }
        }

        for (int y = 0; y < lodWidth - 1; y += 1)
        {
            for (int x = 0; x < lodHeight - 1; x += 1)
            {
                int lowerLeftIndex = lodWidth * y + x;
                int lowerRightIndex = lowerLeftIndex + 1;
                int upperLeftIndex = lowerLeftIndex + lodWidth;
                int upperRightIndex = upperLeftIndex + 1;

                tris.AddRange(new int[] { upperRightIndex, lowerLeftIndex, upperLeftIndex, lowerRightIndex, lowerLeftIndex, upperRightIndex });
            }
        }
        var trisArray = tris.ToArray();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = trisArray;
        mesh.RecalculateNormals();
        return mesh;

    }

    public static Mesh GenerateMeshNoLOD(float[,] noiseMap, float maxHeight, AnimationCurve heightCurve, int worldWidth)
    {
        int width = noiseMap.GetLength(0);

        float unit = (float)worldWidth / (float)width;
        //unit = 1;

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
        //Debug.Log(tmpX);

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

}
