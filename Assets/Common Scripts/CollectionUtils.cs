using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DL.Utils
{
    public static class CollectionUtils
    {
        public static void Loop<T>(this T[,,] input, System.Action<int, int, int> executor)
        {
            for (int x = 0; x < input.GetLength(0); x++)
            {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    for (int z = 0; z < input.GetLength(2); z++)
                    {
                        executor?.Invoke(x, y, z);
                    }
                }
            }
        }
        public static void Loop<T>(this T[,,] input, System.Action<T, int, int, int> executor)
        {
            for (int x = 0; x < input.GetLength(0); x++)
            {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    for (int z = 0; z < input.GetLength(2); z++)
                    {
                        executor?.Invoke(input[x,y,z], x, y, z);
                    }
                }
            }
        }
        public static void Loop<T>(this T[,,] input, System.Action<T> executor)
        {
            for (int x = 0; x < input.GetLength(0); x++)
            {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    for (int z = 0; z < input.GetLength(2); z++)
                    {
                        executor?.Invoke(input[x,y,z]);
                    }
                }
            }
        }
    }
}
