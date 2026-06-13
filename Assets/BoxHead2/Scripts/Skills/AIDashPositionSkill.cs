using System;
using System.Collections;
using BoxHead2.Actor;
using BoxHead2.Combat;
using BoxHead2.Helper;
using BoxHead2.Items;
using BoxHead2.Utils;
using Dacodelaac.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace BoxHead2.Skills
{
    [CreateAssetMenu(menuName = "Skills/AIDashPositionSkill")]
    public class AIDashPositionSkill : Skill
    {
        [SerializeField] bool alwaysTrackPlayer;
        [Header("Animation")]
        [SerializeField] string animation;
        [SerializeField] string stopAnimation;
        [SerializeField] protected float animSpeed = 1;
        [SerializeField] AnimationClip clip;
        [Header("Dashing")] 
        [SerializeField] bool useNonAgentDash;
        [SerializeField] float dashSpeed;
        [SerializeField] bool rotateForward;
        [SerializeField] DashBehaviour dashBehaviour;
        [SerializeField] Vector3 position;
        [SerializeField] float value;
        [SerializeField] float passThroughOffset, horizontalOffset;
        [Header("Attack")]
        [SerializeField] HurtBoxGroup[] hurtBoxGroupPrefabs;
        [SerializeField] protected LayerMaskType damageLayer = LayerMaskType.EnemyHitBoxLayer;
        [SerializeField] MeleeHitData[] hitDatas;
        [SerializeField] float extraDamageFrame;
        [SerializeField] float hurtBoxResetInterval = -1;
        [SerializeField] MaterialModifyIndicator indicatorPrefab;
        [SerializeField] protected ParticleSystem fxSlash;
        [SerializeField] protected float indicatorDuration, indicatorOffset;
        [Header("Feedback")]
        [SerializeField] Feedback dashFeedback, slashFeedback;
        [SerializeField] HitFeedback hitFeedback;
        

        public override bool IsStopConditionMet => string.IsNullOrEmpty(stopAnimation) ? stopped : !Actor.IsPlayingAnim;
        Vector3 MapPosition => Vector3.zero;

        bool _startDash;
        protected Vector3 _startDashPosition;
        
        public bool stopped;
        float updateLastPosTime;
        Vector3 lastPos;
        protected Vector3 dashPosition;
        float _lastTimeClearHurtBox;
        float _currentFrame;
        float _lastFrame;
        protected bool _track;

        protected float _currentIndicatorVal;
        protected MaterialModifyIndicator _activeIndicator;
        protected ParticleSystem _activeSlashFX;
        
        protected HurtBoxGroup[] _hurtBoxGroups; 
        protected HurtBox[] _hurtBoxes;
        protected MeleeHitData _meleeHitData;
        protected CombinedDamageData _combinedDamageData;
        
        protected PathTraverser pathTraverser;
        NavMeshPath traversePath;
        
        bool _shouldClearHurtBox;
        
        Vector3 playerPos => NetworkPlayer.localPlayer.transform.position;

        protected override void OnAttach()
        {
            base.OnAttach();
            Actor.OnDespawnEvent += OnActorDespawn;
        }

        protected override void OnDetach()
        {
            base.OnDetach();
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
        }
        

        void OnActorDespawn(IActor actor)
        {
            if(_activeIndicator != null) pools.Despawn(_activeIndicator.gameObject);
            actor.OnDespawnEvent -= OnActorDespawn;
        }

        protected override void PrepareSkill()
        {
            Actor.StopMovement();
            stopped = false;
            _startDash = false;
            updateLastPosTime = Time.time;
            
            if (hurtBoxGroupPrefabs.Length > 0)
            {
                HurtBoxHelper.SetupHurtBoxes(pools, Actor, hurtBoxGroupPrefabs, out _hurtBoxGroups, out _hurtBoxes);
                SetupCombineDamage(0);
            }

            //RandomWarpAroundPlayer();
            _currentIndicatorVal = 0;
       
            if(indicatorPrefab) _activeIndicator = pools.Spawn(indicatorPrefab);
            if(useNonAgentDash) Actor.DisableMovement();;
        }
        

        protected void SetupCombineDamage(int index)
        {
            if (hitDatas.Length == 0)
            {
                return;
            }
            index = Mathf.Clamp(index, 0, hitDatas.Length - 1);
            _meleeHitData = hitDatas[index];
            _combinedDamageData = CombinedDamageData.Combine(GetDamageSourceData(), _meleeHitData.DamageConfig);
        }

        protected override void DoStop()
        {
            base.DoStop();
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
            HurtBoxHelper.DespawnHurtBoxes(pools, ref _hurtBoxGroups);
            Actor.EnableMovement();
        }

        public override void OnBeginTracking(int index)
        {
            base.OnBeginTracking(index);
            _track = true;
        }

        public override void OnStopTracking(int index)
        {
            base.OnStopTracking(index);
            _track = false;
        }

        protected override IEnumerator IEPerform()
        {
            Actor.OnSkillBeginAttack();
            Actor.PlayAnimation(animation, 0, animSpeed, true, false);
            Actor.SetRootMotionMult(0);
            if (dashFeedback)
            {
                dashFeedback.Play();
            }

            float t = 0;

            yield return new WaitForEndOfFrame();
            
            while (true)
            {
                var dir = alwaysTrackPlayer ? (playerPos - Actor.Position).normalized : GetAimedDirection();
                if (!_startDash)
                {
                    if(_track) Actor.RotateDirection(dir.Set(y: 0f), 20f, true);

                    UpdateIndicator();
                    
                }
                else
                {
                    var maxTime = Vector3.Distance(_startDashPosition, dashPosition) / dashSpeed;
                    if (!useNonAgentDash)
                    {
                        if(t <= maxTime)
                            Actor.MoveDirection(dashPosition - _startDashPosition, dashSpeed);
                        t += Time.deltaTime;
                    }
                    else
                    {
                        t += Time.deltaTime / maxTime;
                        if (traversePath != null && traversePath.status != NavMeshPathStatus.PathInvalid)
                        {
                            Actor.Transform.position = pathTraverser.Interpolate(t);
                        }
                    }
                    
                    if (rotateForward)
                    {
                        Actor.RotateDirection((dashPosition - Actor.Position).Set(y: 0), 1, true);
                    }
                    
                    if (SimpleMath.InRange(Actor.Position, dashPosition, 0.2f)) // reach target position
                    {
                        break;
                    }
                    if (Time.time - updateLastPosTime > 0.5f)
                    {
                        updateLastPosTime = Time.time;
                        if (SimpleMath.InRange(Actor.Position, lastPos, 0.1f)) // if stuck for 0.5s
                        {
                            break;
                        }
                        lastPos = Actor.Position;
                    }

                    _shouldClearHurtBox = hurtBoxResetInterval > 0 && Time.time - _lastTimeClearHurtBox > hurtBoxResetInterval;
                    if (_shouldClearHurtBox)
                    {
                        _lastTimeClearHurtBox = Time.time;
                    }
                    if (hurtBoxGroupPrefabs.Length > 0)
                    {
                        HurtBoxHelper.ExecuteFrameBaseHurtBox(Actor, Actor.TeamConfig.GetLayerMask(damageLayer), animation, clip, 
                            _hurtBoxes, _combinedDamageData, _shouldClearHurtBox, extraDamageFrame, ref _currentFrame, ref _lastFrame,
                            OnHit);
                    }
                }
                yield return null;
            }
            

            stopped = true;

            StopAction();
        }
        protected virtual void OnHit(Vector3 pos, Vector3 dir, HitType hitType, IDamageTaker damageTaker)
        {
            hitFeedback.Play(hitType);
            
        }

        protected virtual void StopAction()
        {
            if (string.IsNullOrEmpty(stopAnimation))
            {
                Actor.OnSkillEndAttack();
                Actor.OnSkillCanMoveNextSkill();
            }
            else
            {
                Actor.PlayAnimation(stopAnimation, 0, 1, true, false);
            }
        }

        protected virtual void UpdateIndicator()
        {
            _currentIndicatorVal += Time.deltaTime / indicatorDuration;
            if (_activeIndicator)
            {
                _activeIndicator.UpdateValues(_currentIndicatorVal);
            }
            if (_track)
            {
                GetDashPosition();
                var dirToDashPosIndicator = (dashPosition + Actor.Transform.right * indicatorOffset - Actor.Position).normalized;

                if (_activeIndicator)
                {
                    _activeIndicator.transform.position = Actor.Position;
                    _activeIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.forward, dirToDashPosIndicator);
                    _activeIndicator.transform.localScale = new Vector3(1, 1, Vector3.Distance(Actor.Position, dashPosition));
                }
            }
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
            if (_activeSlashFX)
            {
                _activeSlashFX.gameObject.SetActive(true);
                _activeSlashFX.Clear();
                _activeSlashFX.Play();
            }

            slashFeedback?.Play();
        }

        protected void GetDashPosition()
        {
            switch (dashBehaviour)
            {
                case DashBehaviour.DashToFarCorner:
                    var aimedPosition = GetAimedPosition();
                    dashPosition.x = aimedPosition.x < MapPosition.x ? 1 : -1;
                    dashPosition.y = 0;
                    dashPosition.z = aimedPosition.z < MapPosition.z ? 1 : -1;
                    dashPosition *= value;
                    dashPosition += MapPosition;
                    break;
                case DashBehaviour.DashToPosition:
                    dashPosition = position;
                    dashPosition += MapPosition;
                    break;
                case DashBehaviour.RandomAroundTarget:
                    aimedPosition = GetAimedPosition();
                    dashPosition = aimedPosition + SimpleMath.RandomOnCircleXZ() * value;
                    if (NavMesh.SamplePosition(dashPosition, out var hitInfo, 10f, NavMesh.AllAreas))
                    {
                        dashPosition = hitInfo.position;
                    }

                    if (Vector3.Distance(dashPosition, aimedPosition) < value - 0.2f)
                    {
                        var dir = dashPosition - aimedPosition;
                        var newDashPos = aimedPosition - dir.normalized * value;
                        if(NavMesh.SamplePosition(newDashPos, out hitInfo, 10f, NavMesh.AllAreas))
                            dashPosition = hitInfo.position;
                    }
                    break;
                case DashBehaviour.PassThroughTarget:
                    aimedPosition = playerPos;
                    dashPosition = aimedPosition +
                                   (aimedPosition - Actor.Position).normalized * passThroughOffset;
                    var perpenVector = new Vector3(dashPosition.z, 0, -dashPosition.x).normalized * horizontalOffset;
                    dashPosition += perpenVector;
                    break;
                case DashBehaviour.DashForward:
                    dashPosition = Actor.Position + Actor.Transform.forward * value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (NavMesh.SamplePosition(dashPosition, out var hit, 30f, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                dashPosition = hit.position;
            }

            if (useNonAgentDash)
            {
                if (NavMesh.Raycast(Actor.Position, dashPosition, out hit, NavMesh.AllAreas))
                {
                    dashPosition = hit.position;
                }
                traversePath ??= new NavMeshPath();
                if (NavMesh.CalculatePath(Actor.Position, dashPosition, NavMesh.AllAreas, traversePath))
                {
                    if (traversePath.status != NavMeshPathStatus.PathInvalid)
                        pathTraverser.SetUp(traversePath.corners);
                }
            }
        }

        public override void OnBeginTrail(int index)
        {
            base.OnBeginTrail(index);
            if (index == 0)
            {
                _startDash = true;
                _startDashPosition = Actor.Position;
                if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
            }
            //GetDashPosition();
            PlaySlashFX(index);
        }

        protected virtual void PlaySlashFX(int index)
        {
            if (hitDatas == null) return;
            if (index >= hitDatas.Length) return;
            var fxPrefab = hitDatas[index].TrailFxPrefab;
            if (!fxPrefab) return;
            var trail = pools.Spawn(fxPrefab, Actor.Transform);
            WeaponVisualSetter.Bind(Actor, trail.gameObject);
            var t1 = hitDatas[index].TrailFxPrefab.transform;
            var t2 = trail.transform;
            t2.localPosition = t1.localPosition;
            t2.localRotation = t1.localRotation;
            trail.Play();
        }
        
    }
    
    
    public enum DashBehaviour
    {
        DashToFarCorner,
        DashToPosition,
        RandomAroundTarget,
        PassThroughTarget,
        DashForward
    }
}