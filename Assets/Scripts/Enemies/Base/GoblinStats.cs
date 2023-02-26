using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using Enemy.Strategies;
using System.Runtime.CompilerServices;

public class GoblinStats : EnemyStats, IPatrol, IChase, IGround
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private Transform castPos;
    [SerializeField] private float castDist, _runSpeed, _walkSpeed, _chaseRange;
    [SerializeField] private Vector2 _patrolArea;

    public Vector3 patrolCenter { get; set; }
    public Vector2 patrolArea { get => _patrolArea; }
    public float runSpeed { get => _runSpeed; set => _runSpeed = value; }
    public float walkSpeed { get => _walkSpeed; set => _walkSpeed = value; }
    public float chaseRange { get => _chaseRange; set => _chaseRange = value; }
    [HideInInspector] public Vector3 groundNormal { get => groundNormalStrategy.groundNormal; }
    private RaycastGroundNormal groundNormalStrategy;
    private ChaseStrategy chaseStrategy;

    protected override void Awake()
    {
        base.Awake();
        chaseStrategy = new ChaseStrategy(chaseRange, atkRange, target, transform);
        groundNormalStrategy = new RaycastGroundNormal(castPos.position, castDist, mask);
    }
    protected override void CustomUpdate()
    {
        chaseStrategy.Excute();
    }

}
