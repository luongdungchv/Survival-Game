using System.Collections.Generic;
using UnityEngine;

namespace Dacodelaac.Utils
{
    public static class ListExtensions
    {
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            (list[i], list[j]) = (list[j], list[i]);
        }

        /// <summary>
        /// Shuffles a list randomly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list, bool useCrytoRandom = false)
        {
            for (var i = 0; i < list.Count; i++)
            {
                list.Swap(i, useCrytoRandom ? CryptoRandom.Range(i, list.Count) : Random.Range(i, list.Count));
            }                
        }
    }
}