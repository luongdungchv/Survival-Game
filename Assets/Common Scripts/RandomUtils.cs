using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DL.Utils
{
    public static class WorldRandomUtils
    {
        public static Vector2 RandomPositionInsideHex(float radius)
        {
            float radiusMiddle = Mathf.Sqrt(3f) / 2;
            float angle = Random.Range(0f, 360);
            var normalizedAngle = (angle - 30) % 60;
            float otherAngle = Mathf.Abs(normalizedAngle - 30);
            var randomLength = radiusMiddle * radius / Mathf.Cos(otherAngle * Mathf.Deg2Rad) * Random.Range(0f, 1f);
            return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * randomLength;
        }
        public static Vector3 RandomPositionInsideHex3D(float radius, float height)
        {
            var hex2D = RandomPositionInsideHex(radius);
            return new Vector3(hex2D.x, height, hex2D.y);
        }
        public static Vector3 RandomPositionInsideCircle3D(float radius, float height)
        {
            var randomPos2D = Random.insideUnitCircle;
            randomPos2D *= radius;
            var randomPos = new Vector3(randomPos2D.x, height, randomPos2D.y);
            return randomPos;
        }
        public static Vector3 RandomPositionInsideRect3D(float sizeX, float sizeZ, float height)
        {
            var (left, right) = (-sizeX / 2, sizeX / 2);
            var (top, bottom) = (sizeZ / 2, -sizeZ / 2);
            var randX = Random.Range(left, right);
            var randZ = Random.Range(bottom, top);
            var randomPos = new Vector3(randX, height, randZ);
            return randomPos;
        }
        public static Vector3 RandomPositionInsideTriangle3D(float radius, float height, int seed = 0)
        {
            Debug.Log(height);
            var randObj = new System.Random(Random.Range(0, 100000));
            float radiusMiddle = 0.5f;
            float angle = Convert.ToSingle(randObj.NextDouble()) * 360;
            var normalizedAngle = (angle + 360 - 30) % 120;
            float otherAngle = Mathf.Abs(normalizedAngle - 60);

            var randOne = Convert.ToSingle(randObj.NextDouble());
            var randTwo = Convert.ToSingle(randObj.NextDouble()) * (1 - randOne - 0.15f);
            var factor = randOne >= 0.5f ? 0 : 1;

            var randomLength = radiusMiddle * radius / Mathf.Cos(otherAngle * Mathf.Deg2Rad) * (randOne + randTwo * factor);
            return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * randomLength + Vector3.up * height;
        }
        public static List<Vector3> RandomCirclesInsideCircle(float bigRadius, float smallRadius, float minDistance, int count, float height = 0)
        {
            var occupationList = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                var randomPos = RandomPositionInsideCircle3D(bigRadius, height);

                bool satisfied = true;
                foreach (var pos in occupationList)
                {
                    var distance = Vector3.Distance(randomPos, pos);
                    if (distance < minDistance)
                    {
                        satisfied = false;
                        break;
                    }
                }

                while (!satisfied)
                {
                    satisfied = true;
                    randomPos = RandomPositionInsideCircle3D(bigRadius, height);
                    foreach (var pos in occupationList)
                    {
                        var distance = Vector3.Distance(randomPos, pos);
                        if (distance < minDistance)
                        {
                            satisfied = false;
                            break;
                        }
                    }
                }

                occupationList.Add(randomPos);
            }
            return occupationList;
        }
    }

    public static class CollectionRandomUtils
    {
        public static List<T> ShuffleCollection<T>(this List<T> input)
        {
            var res = input.OrderBy(x => Random.Range(0, input.Count));
            return res.ToList();
        }
        public static List<T> ShuffleCollection<T>(this List<T> input, int seed)
        {
            if (seed == -1) seed = Random.Range(0, 10000);
            var randObj = new System.Random(seed);
            var res = input.OrderBy(x => randObj.Next(0, input.Count));
            return res.ToList();
        }
        public static List<T> ShuffleCollection<T>(this IEnumerable<T> input, int count, int seed)
        {
            if (seed == -1) seed = Random.Range(0, 10000);
            var randObj = new System.Random(seed);
            var res = input.OrderBy(x => randObj.Next(0, input.Count()));
            // var resList = input.GetRange(0, count);
            // resList = ShuffleCollection(resList, seed);
            return res.ToList().GetRange(0, count);
        }
        public static Queue<T> ShuffleQueue<T>(this Queue<T> input)
        {
            var list = input.ToList();
            list = ShuffleCollection(list);
            var res = new Queue<T>();
            list.ForEach((x) => res.Enqueue(x));
            return res;
        }
        public static Queue<T> ShuffleQueue<T>(this Queue<T> input, int seed)
        {
            var list = input.ToList();
            list = ShuffleCollection(list, list.Count, seed);
            var res = new Queue<T>();
            list.ForEach((x) => res.Enqueue(x));
            return res;
        }
        public static T GetRandomElement<T>(this IEnumerable<T> input, int seed = -1)
        {
            if (seed == -1) seed = Random.Range(0, 100000);
            var count = input.Count();
            if (count == 0) return default;
            if (count == 1) return input.ElementAt(0);
            var randObj = new System.Random(seed);
            var randomIndex = randObj.Next(0, count);
            return input.ElementAt(randomIndex);
        }
        public static (T, int) GetRandomElementWithIndex<T>(this IEnumerable<T> input, int seed = -1)
        {
            if (seed == -1) seed = Random.Range(0, 100000);
            var count = input.Count();
            if (count == 0) return default;
            if (count == 1) return (input.ElementAt(0), 0);
            var randObj = new System.Random(seed);
            var randomIndex = randObj.Next(0, count);
            return (input.ElementAt(randomIndex), randomIndex);
        }


        public static List<int> AscendingOrderList(int listCount, int seed)
        {
            var res = new List<int>();
            for (int i = 0; i < listCount; i++)
            {
                res.Add(i);
            }
            return res;
        }
        public static List<int> RandomOrderList(int listCount, int seed)
        {
            var res = AscendingOrderList(listCount, seed);
            res = ShuffleCollection(res, seed);
            return res;
        }
    }
}
