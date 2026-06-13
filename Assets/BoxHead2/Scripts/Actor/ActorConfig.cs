using System;
using BoxHead2.Combat;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Actor
{
    public class ActorConfig : BaseSO
    {
        [SerializeField] public string title;
        [SerializeField] public string title2;
        [SerializeField] public ActorType type;
        [SerializeField] public ExtraType extraType;
        [SerializeField] public HitType hitType = HitType.Meat;
        [SerializeField] public float walkSpeed;
        [SerializeField] public float runSpeed;
        [SerializeField] public float rotateSpeed;
        [SerializeField] public float acceleration = 8f;
        [SerializeField] public int avoidancePriorityNormal;
        [SerializeField] public int avoidancePriorityAttack;
        [SerializeField] public float enemyCloseRange;
        [SerializeField] public float deadDuration;
        [SerializeField] public bool canBeStagger = true;
        [SerializeField] public bool canBeCinch = true;
        [SerializeField] public bool riseAfterBreak = true;
        [SerializeField] public bool canGetHitAnim = true;
        [SerializeField] public bool canBeSlow = true;
        [SerializeField] public bool canBeKnockBack = true;
        [SerializeField] public bool canBeKnockDown = true;
        [SerializeField] public bool canBePulled = true;
        [SerializeField] public float knockBackDeceleration = 30f;
        [SerializeField, Tooltip("Play Appear state")] public bool needSpawnAnimation = true;
        [SerializeField, Tooltip("Play animation in Appear state")] public bool hasSpawnAnimation = false;
        [SerializeField] public ParticleSystem appearFxPrefab;
        [SerializeField] public float appearFxScale = 1f;
    }
    
    [Flags]
    public enum ActorType
    {
        Player = 1,
        Creep = 2,
        Elite = 4,
        Boss = 8,
        Pet = 16,
        Ally = Player | Pet,
        Enemy = Creep|Elite|Boss,
        None = 128,
        Random = 256,
        Breakable = 512,
    }

    [Flags]
    public enum ExtraType
    {
        Melee = 1,
        Ranged = 2,
    }
}