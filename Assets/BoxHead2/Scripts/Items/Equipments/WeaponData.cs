using System;
using System.Linq;
using System.Text;
using BoxHead2.Actor;
using BoxHead2.Helper;
using BoxHead2.Skills;
using Dacodelaac;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Items
{
    [CreateAssetMenu(menuName = "Items/Equipments/WeaponData")]
    public class WeaponData : BaseSO
    {
        [Header("Weapons")] 
        [SerializeField] int gemSlotBonus;
        [SerializeField] MainWeaponType mainWeaponType;
        [SerializeField] bool isRangedWeapon;
        [SerializeField] int ammoPerBundle;
        [Header("Combo")] [SerializeField] public bool canCombo = true;
        [SerializeField] AttachConfig[] attachConfigs;
        [SerializeField] public bool infiniteCombo;
        [SerializeField] Skill[] normalCombo;
        [Header("Dps")] [SerializeField] public DpsElement[] dpsElements;
        [SerializeField] public float coolDown;

        [Header("AutoHoldAttack")] 
        [SerializeField] public bool canHoldAutoAttack;

        [SerializeField] public bool canAllyAimedBuff;
        public MainWeaponType MainWeaponType => mainWeaponType;

        Skill[] Combo => normalCombo;
        public int Damage => GetDamage();
        public bool IsRangedWeapon => isRangedWeapon;
        public int AmmoPerBundle => ammoPerBundle;
        public AttachConfig[] AttachConfigs => attachConfigs;
        public Skill[] RuntimeCombo { get; protected set; }

        public virtual bool CanUseSkillCombo(int i) => infiniteCombo || (i < (RuntimeCombo?.Length ?? 0));
        // Weapon[] _spawnWeapons;
        Weapon[] _onBodyWeapons;
        protected int _nextSkillIndex;

        bool _lockVisualOnOff;
        

        public override T CreateCopy<T>()
        {
            var weapon = base.CreateCopy<WeaponData>();
            weapon.SetupCombo(Combo);
            return weapon as T;
        }
        
        public virtual void OnUseSkill(Skill skill)
        {
            SetNextSkillIndex(Array.IndexOf(RuntimeCombo, skill) + 1);
        }

        protected virtual void SetNextSkillIndex(int i)
        {
            if (RuntimeCombo.Length > 0)
            {
                _nextSkillIndex = i % RuntimeCombo.Length;
                while (!RuntimeCombo[_nextSkillIndex].Usable)
                {
                    _nextSkillIndex = (_nextSkillIndex + 1) % RuntimeCombo.Length;
                }
            }
        }

        public void ResetSkillCombo()
        {
            _nextSkillIndex = 0;
        }

        public virtual Skill GetNextSkill()
        {
            return RuntimeCombo[_nextSkillIndex];
        }

        public void TurnOnVisualLock(bool isLock)
        {
            _lockVisualOnOff = isLock;
            TurnOnVisualInternal();
        }

        public void TurnOffVisualLock(bool isLock)
        {
            _lockVisualOnOff = isLock;
            TurnOffVisualInternal();
        }

        public void TurnOnVisual()
        {
            if (_lockVisualOnOff) return;
            TurnOnVisualInternal();
        }

        void TurnOnVisualInternal()
        {
            // if (_spawnWeapons != null)
            // {
            //     foreach (var weapon in _spawnWeapons)
            //     {
            //         weapon.Show();
            //     }
            // }

            if (_onBodyWeapons != null)
            {
                foreach (var weapon in _onBodyWeapons)
                {
                    weapon.Show();
                }
            }
        }

        public void TurnOffVisual()
        {
            if (_lockVisualOnOff) return;
            TurnOffVisualInternal();
        }

        void TurnOffVisualInternal()
        {
            // if (_spawnWeapons != null)
            // {
            //     foreach (var weapon in _spawnWeapons)
            //     {
            //         weapon.Hide();
            //     }
            // }

            if (_onBodyWeapons != null)
            {
                foreach (var weapon in _onBodyWeapons)
                {
                    weapon.Hide();
                }
            }
        }
        

        protected virtual void SetupCombo(Skill[] weaponCombo)
        {
            RuntimeCombo = new Skill[weaponCombo.Length];
            for (var i = 0; i < weaponCombo.Length; i++)
            {
                RuntimeCombo[i] = weaponCombo[i].CreateCopy<Skill>();
                RuntimeCombo[i].Initialize();
                RuntimeCombo[i].Attach(null);
            }

            if (RuntimeCombo.Length > 0)
            {
                var tempIndex = RuntimeCombo[0].SourceAttackId;
                for (var index = 1; index < RuntimeCombo.Length; index++)
                {
                    RuntimeCombo[index].SourceAttackId = tempIndex;
                }
            }

            _nextSkillIndex = 0;
        }

        int GetDamage()
        {
            return 1;
        }
        
        Skill[] GetSkill()
        {
            var curSkill = Combo;

            return curSkill;
        }
    }

    [Serializable]
    public class AttachConfig
    {
        [SerializeField] WeaponAttachSlotType slot;
        [SerializeField] Weapon weaponPrefab;

        public WeaponAttachSlotType Slot => slot;
        public Weapon WeaponPrefab => weaponPrefab;
    }

    public enum MainWeaponType
    {
        None = 0,
        OneHanded = 1,
        TwoHanded = 2,
    }
}