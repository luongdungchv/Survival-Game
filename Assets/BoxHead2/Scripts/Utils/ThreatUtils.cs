using UnityEngine;

namespace BoxHead2.Utils
{
    public static class ThreatUtils
    {
        public static float[] EnemyAtkScale = { 1f, 1.2f, 1.6f, 2.1f, 2.9f, 3.7f, 4.8f };
        public static float[] EnemyHpScale = { 1f, 1.7f, 2.7f, 4.1f, 5.5f, 7.4f, 9.9f };
        public static double[] EnemyEXPScale = { 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6 };

        // public static int GetEnemyDamage(int baseDamage, int threat)
        // {
        //     threat = Mathf.Clamp(threat, 0, EnemyAtkScale.Length - 1);
        //     return Mathf.RoundToInt(1f * baseDamage * EnemyAtkScale[threat]);
        // }
        //
        // public static int GetEnemyHp(int baseHp, int threat)
        // {
        //     threat = Mathf.Clamp(threat, 0, EnemyHpScale.Length - 1);
        //     return Mathf.RoundToInt(1f * baseHp * EnemyHpScale[threat]);
        // }
        //
        // public static double GetEnemyEXP(double baseEXP, int threat)
        // {
        //     threat = Mathf.Clamp(threat, 0, EnemyEXPScale.Length - 1);
        //     return baseEXP * EnemyEXPScale[threat];
        // }
    }
}