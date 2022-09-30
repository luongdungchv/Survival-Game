using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float minHeight, maxHeight;

    public static float[,] GenerateNoiseBase(int width, int height, float scale, int octaves, float lacunarity, float persistence, Vector2 offset, float falloffPower)
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
    public static float[,] GenerateNoise(int width, int height, float scale, int octaves, float lacunarity, float persistence, Vector2 offset, AnimationCurve falloff)
    {
        minHeight = float.MaxValue;
        maxHeight = float.MinValue;
        float[,] res = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                float frequency = 1;
                float amplitude = 1;
                //float noiseHeight = -(falloff.Evaluate((float)x / width) + falloff.Evaluate((float)y / height)) / 2;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + offset.x;
                    float sampleY = y / scale * frequency + offset.y;
                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlinVal * amplitude;

                    frequency *= lacunarity;
                    amplitude *= persistence;
                }
                noiseHeight = Mathf.Clamp(noiseHeight, -1, 1);

                res[x, y] = noiseHeight;
            }

        }
        return res;
    }
}
