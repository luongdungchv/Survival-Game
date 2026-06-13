using System;
using System.Collections.Generic;
using System.Linq;
using BoxHead2.Combat;
using BoxHead2.Skills;
using Dacodelaac.Utils;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace BoxHead2.Actor
{
    [CreateAssetMenu(menuName = "ActorConfig/AIConfig")]
    public class AIConfig : ActorConfig
    {
        [SerializeField] public float maxHp = 150f;
        [SerializeField] bool fixedMaxHp;
        [SerializeField] public int dmg = 20;
        [SerializeField] public double exp = 20;
        [SerializeField] float damageReduction = 0;
        [SerializeField] public float attackInterval = 3f;
        [SerializeField] public float firstAttackDelay;
        // [SerializeField] public AssetReference detailRef;
        [SerializeField] public AIConfigDetail configDetail;
        [SerializeField] public string desc;
        [SerializeField] public string[] preSpawnAnims;
        [SerializeField] public float aimToEnemyRange = 15f;
        [SerializeField] public float startAttackRange = 8f;
        [SerializeField] public float stopAttackWhenHit;
        [SerializeField] public bool canAttackWhenGetHit;
        [SerializeField] public bool canDropFlower;
        
        public float MaxHp => maxHp;
        public float Dmg => dmg;
        public string Desc => desc;
        public AIConfigDetail Detail => this.configDetail;

        float GetMaxHp() => MaxHp;

        int GetDamage() => Mathf.RoundToInt(Dmg);
        
        public float GetDamageReduction()
        {
            return damageReduction;
        }

        double GetExp() => this.exp;
        
        public Dictionary<SpecialSkillId, List<SpecialSkill>> CloneSpecialSkills(Actor actor)
        {
            var dict = new Dictionary<SpecialSkillId, List<SpecialSkill>>();
            for (var i = 0; i < Detail.specialSkills.Length; i++)
            {
                if (!dict.ContainsKey(Detail.specialSkills[i].Id))
                {
                    dict.Add(Detail.specialSkills[i].Id, new List<SpecialSkill>());
                }
                var skill = Detail.specialSkills[i].CreateCopy<Skill>();
                skill.Attach(actor);
                dict[Detail.specialSkills[i].Id].Add(skill);
            }

            return dict;
        }

        public SkillCombo[] CloneSkillCombo(Actor actor, bool duplicateSkills)
        {
            var sc = new SkillCombo[Detail.skillCombos.Length];
            for (var i = 0; i < sc.Length; i++)
            {
                sc[i] = Detail.skillCombos[i].CreateCopy(duplicateSkills);
                sc[i].Attach(actor);
            }

            return sc;
        }

        public void SpawnAsync(TeamConfig teamConfig, Vector3 position, Quaternion rotation, int threatLevel, bool isMutant, bool appearAnim, bool canDrop, int zoneIndex, System.Action<AIEnemy> onCompleted, SpawnMoveAnim spawnMoveAnim = null)
        {
            SpawnInternal(teamConfig, position, rotation, threatLevel, isMutant, appearAnim, canDrop, zoneIndex, onCompleted, spawnMoveAnim);
        }
        
        public void SpawnSynced(TeamConfig teamConfig, Vector3 position, Quaternion rotation, int threatLevel, bool isMutant, bool appearAnim, bool canDrop, int zoneIndex, System.Action<AIEnemy> onCompleted, SpawnMoveAnim spawnMoveAnim = null)
        {
            
            SpawnInternal(teamConfig, position, rotation, threatLevel, isMutant, appearAnim, canDrop, zoneIndex, onCompleted, spawnMoveAnim);
        }

        void SpawnInternal(TeamConfig teamConfig, Vector3 position, Quaternion rotation, int threatLevel, bool isMutant, bool appearAnim, bool canDrop, int zoneIndex, System.Action<AIEnemy> onCompleted, SpawnMoveAnim spawnMoveAnim)
        {
            var enemy = pools.Spawn(Detail.aiPrefab, initialize: false);
            enemy.transform.rotation = rotation;
            enemy.EnableMovement();
            enemy.transform.position = position;
            enemy.Warp(position);

            var stat = new EnemyStat(
                0.05f,
                0.5f,
                GetDamage(),
                GetDamageReduction(),
                GetMaxHp(),
                isMutant, GetExp());
            
            enemy.EnemyStat = stat;
            enemy.CanDrop = canDrop;
            enemy.ZoneIndex = zoneIndex;

            if (spawnMoveAnim == null || !spawnMoveAnim.SpawnAnim)
            {
                enemy.SetConfig(this, teamConfig);
                pools.Initialize(enemy.gameObject);
                if (appearAnim && needSpawnAnimation)
                {
                    enemy.OnAppear();
                }
                onCompleted?.Invoke(enemy);
            }
            else
            {
                enemy.TurnOffHitBox();
                DOTween.To(() => position, x =>
                {
                    enemy.Warp(x);
                }, spawnMoveAnim.PosTarget, spawnMoveAnim.Duration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    enemy.SetConfig(this, teamConfig);
                    pools.Initialize(enemy.gameObject);
                    enemy.DelayTurnOnHitBox(spawnMoveAnim.DelayHitBox);
                    onCompleted?.Invoke(enemy);
                }).Play();
            }
        }
        [Sirenix.OdinInspector.Button]
        void SpawnTest(TeamConfig teamConfig)
        {
            var playerPos = NetworkPlayer.localPlayer.transform.position;
            var spawnPos = playerPos + SimpleMath.RandomInCircleXZ() * 12;
            if (NavMesh.SamplePosition(spawnPos, out var hit, 100, NavMesh.AllAreas))
            {
                SpawnAsync(teamConfig, hit.position, Quaternion.identity, 1, false, false, false, 0, null);
            }
        }
    }

    [Serializable]
    public class SpecialSkill
    {
        [SerializeField] SpecialSkillId id;
        [SerializeField] SkillWrapperCondition condition;
        [SerializeField] Skill skill;

        public SpecialSkillId Id => id;
        public Skill Skill => skill;
        public SkillWrapperCondition Condition => condition;

        public SpecialSkill CreateCopy<T>()
        {
            var sk = new SpecialSkill();
            sk.id = id;
            sk.skill = skill.CreateCopy<Skill>();
            sk.condition = condition.CreateCopy();
            return sk;
        }

        public void Attach(Actor actor)
        {
            skill.Attach(actor);
        }

        public void Detach()
        {
            skill.Detach();
        }
    }
    
    public enum SpecialSkillId
    {
        Retreat,
        Revive,
        AfterDie,
        PrepareDie,
        AfterSpawn,
        Wakeup,
        ChangeState
    }

    [Serializable]
    public class SkillCombo
    {
        [SerializeField] public string skillName;
        [SerializeField] int weight = 1;
        [SerializeField] float initialCooldown;
        [SerializeField] float cooldown = 0;
        [SerializeField] SkillWrapperCondition condition;
        [SerializeField] Skill[] skills;
        [SerializeField] bool isSameAllyUse;

        public int Weight => weight;
        public SkillWrapperCondition Condition => condition;
        public Skill[] Skills => skills;
        float elapsed;
        public bool Available => elapsed >= cooldown || cooldown == 0;
        public bool IsForcePlayWhenConditionMatch { get; set; }
        public bool IsSameAllyUse => isSameAllyUse;
        
        public SkillCombo()
        {
        }

        public SkillCombo(Skill[] skills)
        {
            this.skills = skills;
        }

        public SkillCombo CreateCopy(bool duplicateSkills = false)
        {
            var sc = new SkillCombo();
            sc.weight = weight;
            sc.condition = condition.CreateCopy();
            
            var skillsLength = skills.Length * (duplicateSkills ? 2 : 1);
            
            sc.skills = new Skill[skillsLength];
            sc.elapsed = cooldown - initialCooldown;
            sc.cooldown = cooldown;
            sc.skillName = skillName;
            sc.isSameAllyUse = isSameAllyUse;
            for (var i = 0; i < skillsLength; i++)
            {
                sc.skills[i] = skills[i % skills.Length].CreateCopy<Skill>();
            }
            sc.IsForcePlayWhenConditionMatch = sc.condition.IsForcePlayWhenMatch;
            
            return sc;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (elapsed < cooldown)
            {
                elapsed += deltaTime;
            }
        }

        public void ResetCooldown()
        {
            elapsed = 0;
        }
        
        public void Attach(Actor actor)
        {
            foreach (var skill in skills)
            {
                skill.Attach(actor);
            }
        }

        public void Detach()
        {
            foreach (var skill in skills)
            {
                skill.Detach();
            }
        }
    }

    [Serializable]
    public class SkillWrapperCondition
    {
        [SerializeField] bool use = true;
        [SerializeField] Condition[] conditions;

        public bool IsForcePlayWhenMatch => conditions.Any(c => c.IsForcePlayWhenMatch);

        public SkillWrapperCondition CreateCopy()
        {
            var condition = new SkillWrapperCondition();
            condition.use = use;
            condition.conditions = new Condition[conditions.Length];
            for (var i = 0; i < conditions.Length; i++)
            {
                condition.conditions[i] = new Condition();
                condition.conditions[i].type = conditions[i].type;
                condition.conditions[i].operatorCondition = conditions[i].operatorCondition;
                condition.conditions[i].value = conditions[i].value;
            }
            return condition;
        }

        public bool IsMatchCondition(Actor aiEnemy)
        {
            if (!use) return false;
            foreach (var condition in conditions)
            {
                if (!condition.IsMatch(aiEnemy))
                {
                    return false;
                }
            }
            return true;
        }

        public float GetConditionValue(int i)
        {
            return conditions.Length > i ? conditions[i].value : 0f;
        }

        [Serializable]
        public class Condition
        {
            [SerializeField] public Type type;
            [SerializeField] public Operator operatorCondition;
            [SerializeField] public float value;

            public bool IsMatch(Actor actor)
            {
                var aiEnemy = actor as AIEnemy;
                switch (type)
                {
                    case Type.HpRatio:
                        return IsMatch(actor.HpRatio);
                    case Type.EnemyDistance:
                        return IsMatch(actor.GetEnemyDistance());
                    case Type.SkillCondition1:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.GetCustomSkillCondition(1));
                    case Type.Phase:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.GetPhase());
                    case Type.Mutant:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.EnemyStat.IsMutant ? 1 : 0);
                    case Type.Difficulty:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.EnemyStat.Difficulty);
                    case Type.EnemyStun:
                        return actor.AimedEnemy != null && IsMatch(actor.IsEnemyStunned());
                    case Type.SkillCondition2:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.GetCustomSkillCondition(2));
                    case Type.SkillCondition3:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.GetCustomSkillCondition(3)); 
                    case Type.SkillCondition4:
                        if (!aiEnemy) return false;
                        return IsMatch(aiEnemy.GetCustomSkillCondition(4)); 
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            bool IsMatch(float v)
            {
                switch (operatorCondition)
                {
                    case Operator.GreaterOrEqual:
                        return v >= value;
                    case Operator.Lesser:
                        return v < value;
                    case Operator.EqualInt:
                        return Mathf.RoundToInt(v) == Mathf.RoundToInt(value);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(operatorCondition), operatorCondition, null);
                }
            }

            bool IsMatch(bool v)
            {
                switch (operatorCondition)
                {
                    case Operator.True:
                        return v;
                    case Operator.False:
                        return !v;
                    default:
                        return false;
                }
            }

            public bool IsForcePlayWhenMatch => type == Type.HpRatio && operatorCondition == Operator.Lesser ||
                                                type == Type.Phase && operatorCondition == Operator.GreaterOrEqual ||
                                                type == Type.SkillCondition4;

            public enum Type
            {
                HpRatio,
                EnemyDistance,
                SkillCondition1,
                Phase,
                Difficulty,
                Mutant,
                SkillCondition2,
                EnemyStun,
                SkillCondition3,
                SkillCondition4,
                
            }

            public enum Operator
            {
                GreaterOrEqual,
                Lesser,
                EqualInt,
                True,
                False
            }
        }
    }
    public class SpawnMoveAnim
    {
        public bool SpawnAnim;
        public Vector3 PosTarget;
        public float Duration;
        public float DelayHitBox;
    }
}