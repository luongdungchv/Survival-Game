using System;
using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Collection;
using BoxHead2.Combat;
using BoxHead2.Items;
using BoxHead2.SubActions;
using BoxHead2.SubCombat;
using Dacodelaac.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace BoxHead2.Skills
{
    public class Missile : BaseMono, ISubActionDataProvider<MissileSetupActionData>,
        ISubActionDataProvider<MissileHitActionData>, ISubActionDataProvider<ExplodeActionData>,
        ISubActionDataProvider<AreaOfEffectActionData>,ISubActionDataProvider<AreaOfEffectWallActionData>
    {
        [SerializeField] MissileCollection collection;
        [SerializeField] protected bool ignoreObstacle;
        [SerializeField] protected Feedback flyFeedback;
        [SerializeField] bool invokeHitOnEnemy;
        [SerializeField] protected LayerMask obstacleMask;

        public Vector3 CurrentDirection { get; set; }
        public DamageSourceData SourceData { get; set; }
        
        protected bool Shooted;
        public bool Despawned { get; set; }
        
        protected MissileFlyAction flyAction;
        protected SubAction hitAction;
        protected SubAction shootAction;
        protected SubAction despawnAction;
        protected float _radiusMissile;
        Action<MissileHitData> OnHitAction;
        float _mulScale = 1f;
        float _speedScale;
        Vector3 _lastPosition;
        Collider _collider;
        int _index;
        int _totalHitEnemyCount;
        

        Action<int> _totalHitEnemy;

        public MissileSetupActionData SetupActionData;
        protected MissileHitActionData missileHitActionData;
        public IActor Actor { get; set; }
        
        HashSet<IDamageTaker> _damageTakerSet = new ();


        public override void BindVariable()
        {
            base.BindVariable();
            if (collection)
            {
                collection.Add(this);
            }
        }

        public override void UnbindVariable()
        {
            base.UnbindVariable();
            if (collection)
            {
                collection.Remove(this);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            Shooted = false;
            Despawned = false;
            transform.localScale = Vector3.one;
            _mulScale = 1f;
            _damageTakerSet = new HashSet<IDamageTaker>();
            // reduce solverIterations in settings to 4, change missile to 6
            var rigid = GetComponent<Rigidbody>();
            if (rigid)
            {
                rigid.solverIterations = 10;
            }

            _collider = GetComponentInChildren<Collider>();
            if (_collider != null)
            {
                if (_collider is SphereCollider sp)
                {
                    _radiusMissile = sp.radius;
                }
                else if (_collider is CapsuleCollider cs)
                {
                    _radiusMissile = cs.radius;
                }
                else
                {
                    _radiusMissile = 0.2f;
                }
            }
            else
            {
                _radiusMissile = 0.2f;
            }
            flyFeedback?.Play();
        }

        public virtual void Setup(MissileFlyAction flyAction, SubAction hitAction, SubAction shootAction, SubAction despawnAction,
            MissileSetupData setupData, DamageSourceData sourceData, Action<MissileHitData> onHitAction, int index, Action<int> totalHitEnemy)
        {
            this.SetupActionData = new MissileSetupActionData(this, setupData);
            this.SourceData = sourceData;
            this.Actor = setupData.Actor;
            _index = index;
            OnHitAction = onHitAction;
            _totalHitEnemyCount = 0;
            _totalHitEnemy = totalHitEnemy;
            _speedScale = 0;
            if (shootAction)
            {
                this.shootAction = shootAction.CreateCopy<SubAction>();
                this.shootAction.Prepare(this);
            }
            if (flyAction)
            {
                this.flyAction = flyAction.CreateCopy<MissileFlyAction>();
                this.flyAction.Prepare(this);
            }
            if (hitAction)
            {
                this.hitAction = hitAction.CreateCopy<SubAction>();
                this.hitAction.Prepare((this));
            }
            if (despawnAction)
            {
                this.despawnAction = despawnAction.CreateCopy<SubAction>();
                this.despawnAction.Prepare(this);
            }

            var missileComponents = GetComponentsInChildren<IMissileComponent>(true);
            foreach (var component in missileComponents)
            {
                component.Setup();
            }
            WeaponVisualSetter.Bind(Actor, gameObject);
            _lastPosition = transform.position;
        }
        
        public void ResetHit()
        {
            if (hitAction)
            {
                hitAction.Prepare((this));
                hitAction.Swap();
            }
        }

        public void Shoot()
        {
            Shooted = true;
            if (shootAction)
            {
                shootAction.Trigger(this, Actor);
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            if (!Shooted) return;
            if (flyAction)
            {
                flyAction.Trigger(this, Actor);
            }

            if (_speedScale > 0 && _mulScale < 3f)
            {
                _mulScale += _speedScale * Time.deltaTime;
                _mulScale = Mathf.Min(_mulScale, 3f);
                transform.localScale = Vector3.one * _mulScale;
            }
            
            RaycastCheck();
        }

        public void Despawn()
        {
            if (Despawned) return;
            Despawned = true;
            if (!invokeHitOnEnemy)
            {
                OnHitAction?.Invoke(new MissileHitData
                {
                    MissileIndex = _index,
                    HitPos = transform.position
                });
            }

            if (despawnAction)
            {
                despawnAction.Trigger(this, Actor);
            }

            if (flyAction)
            {
                flyAction.OnMissileDeSpawn(this);
            }
            _totalHitEnemy?.Invoke(_totalHitEnemyCount);

            _totalHitEnemy = null;
            shootAction = null;
            flyAction = null;
            hitAction = null;
            despawnAction = null;
            pools.Despawn(gameObject);
            flyFeedback?.Stop();
        }

        public void OnBlocked()
        {
            Despawn();
        }

        protected virtual void RaycastCheck()
        {
            var dir = (transform.position - _lastPosition).normalized;
            var distance = Vector3.Distance(transform.position, _lastPosition);
            if (Physics.SphereCast(_lastPosition, _radiusMissile, dir, out var hit, distance, Actor.TeamConfig.GetLayerMask(LayerMaskType.EnemyHitBoxLayer)))
            {
                if (!Shooted || Despawned) return;
                var damageTaker = hit.collider.GetComponentInParent<IDamageTaker>();
                if (damageTaker is { Alive: true })
                {
                    OnHit(damageTaker, hit.transform.position);
                    flyFeedback?.Stop();
                }
            }

            if (!ignoreObstacle && Shooted && !Despawned)
            {
                if (Physics.SphereCast(_lastPosition, _radiusMissile, dir, out hit, distance, obstacleMask))
                {
                    if (flyAction)
                    {
                        missileHitActionData = new MissileHitActionData(null, hit.point, CurrentDirection, hit.normal, SourceData);
                        flyAction.OnHitObstacle(this);
                        flyFeedback?.Stop();
                    }
                }
            }
            _lastPosition = transform.position;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!Shooted || Despawned) return;
            var pos = Transform.position;
            if (other is MeshCollider m && !m.convex)
            {
            }
            else
            {
                pos = other.ClosestPoint(pos);
            }
            // if (!ignoreObstacle)
            // {
            //     if (other.gameObject.layer == LayerMask.NameToLayer("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            //     {
            //         if (flyAction)
            //         {
            //             missileHitActionData = new MissileHitActionData(null, pos, CurrentDirection, Vector3.zero, SourceData);
            //             flyAction.OnHitObstacle(this);
            //         }
            //         return;
            //     }
            // }

            var damageTaker = other.GetComponentInParent<IDamageTaker>();
            if (damageTaker is { Alive: true })
            {
                OnHit(damageTaker, pos);
            }

            other.GetComponent<ColliderHitFx>()?.OnHit(pos, CurrentDirection);
        }

        protected void OnHit(IDamageTaker damageTaker, Vector3 hitPosition)
        {
            missileHitActionData = new MissileHitActionData(damageTaker, hitPosition, CurrentDirection, Vector3.zero, SourceData);
            if (hitAction)
            {
                hitAction.Trigger(this, Actor);
            }
            if (flyAction)
            {
                flyAction.OnHit(this);
            }
            
            if (invokeHitOnEnemy)
            {
                OnHitAction?.Invoke(new MissileHitData
                {
                    MissileIndex = _index,
                    HitPos = transform.position
                });
            }

            if (_damageTakerSet.Add(damageTaker))
            {
                _totalHitEnemyCount++;
            }
        }

        MissileSetupActionData ISubActionDataProvider<MissileSetupActionData>.Get()
        {
            return SetupActionData;
        }

        MissileHitActionData ISubActionDataProvider<MissileHitActionData>.Get()
        {
            return missileHitActionData;
        }

        public ExplodeActionData Get()
        {
            return new ExplodeActionData(Transform.position, SetupActionData.SetupData.TargetPos, SetupActionData.SetupData.RadiusBonus, SourceData, new DamageConfig(), false);
        }
        
        AreaOfEffectActionData ISubActionDataProvider<AreaOfEffectActionData>.Get()
        {
            return new AreaOfEffectActionData(Transform, Transform.position, SourceData, SetupActionData.SetupData.RadiusBonus, Transform.eulerAngles.y);
        }

        AreaOfEffectWallActionData ISubActionDataProvider<AreaOfEffectWallActionData>.Get()
        {
            return new AreaOfEffectWallActionData(Transform, SourceData, () => Despawned, SetupActionData.SetupData.Range, SetupActionData.SetupData.Direction);
        }
        
        void OnDrawGizmos()
        {
            Gizmos.DrawRay(Transform.position, CurrentDirection * 10);
        }
    }

    public struct MissileSetupData
    {
        public IActor Actor;
        public Vector3 Source;
        public Vector3 Direction;
        public IDamageTaker Target;
        public Vector3 TargetPos;
        public float Range;
        public float RadiusBonus;
    }
}