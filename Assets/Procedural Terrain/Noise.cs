using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float minHeight, maxHeight;

    public static float[,] GenerateNoiseBase(int width, int height, float scale, Vector2 offset, float falloffPower = 1)
    {
        minHeight = float.MaxValue;
        maxHeight = float.MinValue;
        float[,] res = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                float sampleX = x / scale + offset.x;
                float sampleY = y / scale + offset.y;
                float perlinVal = Mathf.PerlinNoise(sampleX, sampleY);

                float falloffX = x / (float)width * 2 - 1;
                float falloffY = y / (float)height * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(falloffX), Mathf.Abs(falloffY));
                float falloffVal = Mathf.Pow(value, falloffPower);

                res[x, y] = perlinVal - falloffVal;
                if (perlinVal < minHeight) minHeight = perlinVal;
                if (perlinVal > maxHeight) maxHeight = perlinVal;
            }

        }
        return res;
    }
    public static float[,] GenerateNoiseDiscrete(int width, int height, float scale, Vector2 offset, float threshold)
    {
        float[,] res = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                float sampleX = x / scale + offset.x;
                float sampleY = y / scale + offset.y;
                float perlinVal = Mathf.PerlinNoise(sampleX, sampleY);
                res[x, y] = Step(threshold, perlinVal);

            }

        }
        return res;
    }
    private static int Step(float threshold, float value)
    {
        int res = value >= threshold ? 1 : 0;
        return res;
    }
}
