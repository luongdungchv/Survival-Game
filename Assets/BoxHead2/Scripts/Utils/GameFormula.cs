using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Utils
{
    public static class GameFormula
    {
        static int[,] minPowerMatrix = new int[,]
        {
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 6, 6, 6, 6, 6, 6 },
            { 1, 6, 12, 12, 12, 12, 12 },
            { 1, 6, 12, 18, 18, 18, 18 },
            { 1, 6, 12, 18, 24, 24, 24 },
            { 1, 6, 12, 18, 24, 30, 30 },
            { 1, 6, 12, 18, 24, 30, 35 },
        };
        static int[,] maxPowerMatrix = new int[,]
        {
            { 8, 11, 13, 16, 19, 22, 25 },
            { 11, 13, 16, 19, 22, 25, 28 },
            { 13, 16, 19, 22, 25, 28, 31 },
            { 16, 19, 22, 25, 28, 31, 34 },
            { 19, 22, 25, 28, 31, 34, 37 },
            { 22, 25, 28, 31, 34, 37, 40 },
            { 25, 28, 31, 34, 37, 40, 43 },
        };

        public static float expCoeffA = 60;
        public static float expCoeffB = 6;
        
        public static double GetPlayerEXPRequired(int level)
        {
            if (level == 0) return -1;
            if(level == 1) return 0;
            
            var x = level;
            var a = expCoeffA;
            var b = expCoeffB;

            return (b / 6) * x * x * x + (a + b) / 2 * x * x + (300 - a / 2 - 2 * b / 3) * x - 300;
        }

        public static int[] ShaperPower = { 1, 4, 10, 16, 22, 27, 33 };

        public static int GetGearPower(int threat, int heroPower)
        {
            var threatIndex = ShaperPower.Length - 1;
            while (threatIndex > 0 && heroPower < GetShaperPower(threatIndex))
            {
                threatIndex--;
            }
            
            threatIndex = Mathf.Clamp(threatIndex, 0, ShaperPower.Length - 1);
            threat = Mathf.Clamp(threat, 0, ShaperPower.Length - 1);
            
            return CryptoRandom.Range(minPowerMatrix[threatIndex, threat], maxPowerMatrix[threatIndex, threat]);
        }

        public static int GetShaperPower(int threat)
        {
            return ShaperPower[Mathf.Clamp(threat, 0, ShaperPower.Length - 1)];
        }
    }
}