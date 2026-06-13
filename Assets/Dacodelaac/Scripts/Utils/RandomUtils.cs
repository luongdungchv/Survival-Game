using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Dacodelaac.Utils
{
    public static class RandomUtils
    {
        public static int Range(int inclusive, int exclusive, bool cryptoRandom)
        {
            return cryptoRandom ? CryptoRandom.Range(inclusive, exclusive) : Random.Range(inclusive, exclusive);
        }

        public static T[] GetWeightedRandomItems<T>(T[] items, int[] weights, int count, bool cryptoRandom)
        {
            var result = new List<T>();
            var itemList = items.ToList();
            var weightList = weights != null ? weights.ToList() : Enumerable.Repeat(1, itemList.Count).ToList();
            var totalWeight = 0;

            for (var i = 0; i < itemList.Count; i++)
            {
                totalWeight += weightList[i];
            }

            count = Mathf.Clamp(count, 0, itemList.Count);

            for (var i = 0; i < count; i++)
            {
                var randomValue =
                    cryptoRandom ? CryptoRandom.Range(1, totalWeight + 1) : Random.Range(1, totalWeight + 1);

                for (var j = 0; j < itemList.Count; j++)
                {
                    randomValue -= weightList[j];

                    if (randomValue <= 0)
                    {
                        var item = itemList[j];
                        result.Add(item);
                        totalWeight -= weightList[j];
                        weightList.RemoveAt(j);
                        itemList.RemoveAt(j);
                        break;
                    }
                }
            }

            return result.ToArray();
        }

        public static string GenerateRandomString(int length)
        {
            var stringChars = new char[length];
            GenerateCharArrayKey(ref stringChars);

            return new string(stringChars);
        }

        public static byte GenerateByteKey()
        {
            return (byte)CryptoRandom.Range(100, 255);
        }

        internal static sbyte GenerateSByteKey()
        {
            return (sbyte)CryptoRandom.Range(100, 127);
        }

        internal static char GenerateCharKey()
        {
            return (char)CryptoRandom.Range(10000, 60000);
        }

        internal static short GenerateShortKey()
        {
            return (short)CryptoRandom.Range(10000, short.MaxValue);
        }

        internal static ushort GenerateUShortKey()
        {
            return (ushort)CryptoRandom.Range(10000, ushort.MaxValue);
        }

        internal static int GenerateIntKey()
        {
            return CryptoRandom.Range(1000000000, int.MaxValue);
        }

        internal static uint GenerateUIntKey()
        {
            return (uint)GenerateIntKey();
        }

        internal static long GenerateLongKey()
        {
#if !ACTK_US_EXPORT_COMPATIBLE
            return CryptoRandom.NextLong(1000000000000000000, long.MaxValue);
#else
            return GenerateIntKey();
#endif
        }

        internal static ulong GenerateULongKey()
        {
            return (ulong)GenerateLongKey();
        }

        internal static void GenerateCharArrayKey(ref char[] arrayToFill)
        {
            if (arrayToFill == null)
            {
                arrayToFill = new char[7];
            }
            else if (arrayToFill.Length < 7)
            {
                arrayToFill = new char[7];
            }

            CryptoRandom.NextChars(arrayToFill);
        }
        
        public static List<Vector3> NewGetDropPosition(Vector3 pos, int count, Vector3[] exceptPositions)
        {
            var dropPos = new List<Vector3>();
            for (var i = 0; i < count; ++i)
            {
                var findingPos = false;
                var trying = 5;
                do
                {
                    var newDropPos = pos + SimpleMath.RandomInCircleXZ() * Random.Range(0.5f, 1.3f);

                    if (NavMesh.SamplePosition(newDropPos, out var hit, 2f, 1 << NavMesh.GetAreaFromName("Walkable")))
                    {
                        dropPos.Add(hit.position);
                        findingPos = true;
                    }

                    trying--;
                }
                while (!findingPos && trying > 0);
            }

            if (count > 0 && dropPos.Count == 0)
            {
                dropPos.Add(pos);
            }
            
            while (dropPos.Count < count)
            {
                dropPos.Add(dropPos[^1]);
            }
            
            return dropPos;
        }

    }
}