namespace Dacodelaac.Utils
{
    public partial class CryptoRandom
    {
        static CryptoRandom instance;
        static CryptoRandom Instance = instance ??= new CryptoRandom(true);
        
        /// <summary>
        ///   <para>Return a random int within [minInclusive..maxExclusive) (Read Only).</para>
        /// </summary>
        /// <param name="minInclusive"></param>
        /// <param name="maxExclusive"></param>
        public static int Range(int minInclusive, int maxExclusive)
        {
            if (minInclusive > maxExclusive)
            {
                (minInclusive, maxExclusive) = (maxExclusive, minInclusive);
            }
            return Instance.Next(minInclusive, maxExclusive);
        }

        public static float Range(float min, float max)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }
            return min + value * (max - min);
        }
        
        public static long NextLong(long minInclusive, long maxExclusive)
        {
            var result = (long)Instance.Next((int)(minInclusive >> 32), (int)(maxExclusive >> 32));
            result <<= 32;
            result |= (uint)Instance.Next((int)minInclusive, (int)maxExclusive);
            return result;
        }
        
        public static void NextChars(char[] buffer)
        {
            for (var i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = (char) (Instance.Next() % 256);
            }
        }
        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A float-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        public static float value => (float)Instance.NextDouble();
    }
}