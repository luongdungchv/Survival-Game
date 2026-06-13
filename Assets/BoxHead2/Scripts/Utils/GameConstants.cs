using UnityEngine;

namespace BoxHead2.Utils
{
    public static class GameConstants
    {
        public const int ThreatPerDifficulty = 5;
        public const int NumberOfDifficulty = 4;
        public const int DifficultyPerGroup = 4;
        public const int OldThreatCount = 7;
        public static float EnemyPatrolInterval = 5f;
        public static int TrueDifficultyIndicesCount => NumberOfDifficulty * ThreatPerDifficulty;
        public const string LocKeyTextLockFeatureRuneSlot = "text_lock_rune_slot";
        public const string LocKeyTextLockNormalRuneSlot = "UNLOCK AT +";

        public const int CommonCappedValue = 1000000000;
        public const int TowerFloorCap = 600;

        public static int GetFakeExpValue(double trueExp)
        {
            return Mathf.RoundToInt((float)trueExp * 8);
        }

        public const int MissionLogMMP = 5;
    }
}