using UnityEngine;

namespace System
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance;
        
        [SerializeField] private int dayTimeSpawnChance;
        [SerializeField] private int baseNightTimeSpawnChance;
        [SerializeField] private int addSpawnChancePerDay;

        private void Awake()
        {
            Instance = this;
        }
        
        public int GetNightTimeSpawnChance() => baseNightTimeSpawnChance + DayNightCircle.ins.CurrentDay * addSpawnChancePerDay;
        public int DayTimeSpawnChance => dayTimeSpawnChance;
    }
}