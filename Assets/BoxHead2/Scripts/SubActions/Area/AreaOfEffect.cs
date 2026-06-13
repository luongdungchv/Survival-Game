using System;
using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Collection;
using BoxHead2.Combat;
using BoxHead2.Skills;
using Dacodelaac.Core;
using DG.Tweening;
using UnityEngine;

namespace BoxHead2.SubActions
{
    public class AreaOfEffect : BaseMono, ISubActionDataProvider<ExplodeActionData>
    {
        [SerializeField] CircleObstacleCollection circleObstacleCollection;
        [SerializeField] HurtBox hurtBox;
        [SerializeField] ParticleSystem fxPrefab;
        [SerializeField] Vector3 fxOffset = Vector3.zero;
        [SerializeField] Transform scaleAoe;
        [SerializeField] bool depspawnFxImmediately = false;
        [SerializeField] Feedback spawnFeedback;
        [SerializeField] Feedback loopFeedback;
        [SerializeField] float playLoopFeedbackInterval;
        [SerializeField] protected HitFeedback hitFeedback;
        [SerializeField] bool isPooling = true;
        [SerializeField] bool useDissolveDespawn;
        [SerializeField] Renderer[] renderers;
        
        public event System.Action OnDepawnedEvent;

        protected AreaOfEffectData AreaOfEffectData;
        protected DamageSourceData SourceData;
        protected LayerMask damageLayer;
        
        SubAction endAction;
        float time;
        ParticleSystem fx;
        protected int tickCount;
        Transform followTarget;
        float lastTimePlayLoopFeedback;
        int totalTick;
        CircleObstacle circleObstacle;
        bool _isDeSpawned;

        List<DamageConfig> damageConfigs = new();

        public virtual void Setup(Transform followTarget, AreaOfEffectData areaOfEffectData, DamageSourceData sourceData, LayerMask damageLayer, SubAction endAction)
        {
            _isDeSpawned = true;
            this.endAction = endAction;
            this.followTarget = followTarget;
            this.AreaOfEffectData = areaOfEffectData;
            this.SourceData = sourceData;
            this.damageLayer = damageLayer;
            hurtBox.OverrideRadius(AreaOfEffectData.Radius);
            
            //Calculate Total Tick
            totalTick = Mathf.RoundToInt(AreaOfEffectData.DamageData.Length);
            damageConfigs = new List<DamageConfig>();
            var interval = totalTick - AreaOfEffectData.DamageData.Length;
            for (var i = 0; i < interval; ++i)
            {
                damageConfigs.Add(AreaOfEffectData.DamageData[0]);
            }
            damageConfigs.AddRange(AreaOfEffectData.DamageData);

            if (fxPrefab)
            {
                fx = pools.Spawn(fxPrefab, transform);
                fx.transform.localPosition = Vector3.zero;
                fx.transform.localPosition += fxOffset;
                fx.transform.localScale = Vector3.one * AreaOfEffectData.Radius;
                fx.transform.localEulerAngles = Vector3.zero;
                fx.Play();
            }
            if (spawnFeedback)
            {
                spawnFeedback.Play();
            }

            if (scaleAoe)
            {
                DOTween.Kill(this);
                if (useDissolveDespawn)
                {
                    for (var i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].material.SetFloat("_Cutoff", 0);
                    }
                }
                scaleAoe.localScale = Vector3.one * AreaOfEffectData.Radius;
            }
            
            if (circleObstacleCollection && sourceData.DamageSource.TeamConfig.Index == 1)
            {
                var pos = Transform.position;
                circleObstacle = new CircleObstacle()
                {
                    Center = pos,
                    Radius = AreaOfEffectData.Radius
                };
                circleObstacleCollection.Add(circleObstacle);
            }

            _isDeSpawned = false;
            time = 0f;
            tickCount = 0;
            DealDamage();
            tickCount++;
        }

        public override void Tick()
        {
            if (_isDeSpawned) return;
            time += Time.deltaTime;
            if (followTarget)
            {
                Transform.position = followTarget.position;
                Transform.rotation = followTarget.rotation;
            }

            if (loopFeedback && Time.time - lastTimePlayLoopFeedback > playLoopFeedbackInterval)
            {
                lastTimePlayLoopFeedback = Time.time;
                loopFeedback.Play();
            }
            
            //Compare with total tick
            while (time >= AreaOfEffectData.TickInterval && tickCount < damageConfigs.Count)
            {
                DealDamage();
                tickCount++;
                time -= AreaOfEffectData.TickInterval;
            }
            if (tickCount >= damageConfigs.Count)
            {
                Despawn(depspawnFxImmediately);
            }
        }

        protected virtual void DealDamage()
        {
            // Debug.Log(tickCount + 1);
            hurtBox.Group.DamageTakers.Clear();
            var hits = hurtBox.Scan(damageLayer);
            foreach (var hit in hits)
            {
                var position = Transform.position;
                var direction = hurtBox.explosionForce ? hit.DamageTaker.Position - position : Transform.forward;
                direction.y = 0;
                direction.Normalize();
                
                //Get index tick
                var damageConfig = GetDamageConfig(tickCount);
                var combinedDamage = CombinedDamageData.Combine(SourceData, damageConfig);
                combinedDamage.Damage *= hit.DamageScale;
                combinedDamage.UpdateDamageForce(direction, hit.HitPosition, position);
                if (hit.OverrideDamageForce)
                {
                    combinedDamage.OverrideDamageForce(hit.DamageForce);
                }
                var hitType = hit.DamageTaker.TakeDamage(combinedDamage);
                hitFeedback.Play(hitType.HitType);
                OnDealDamage(hit.DamageTaker, hitType.HitType);
            }
        }

        protected virtual void OnDealDamage(IDamageTaker damageTaker, HitType hitType)
        {
            
        }

        protected virtual DamageConfig GetDamageConfig(int tick)
        {
            tick = Mathf.Clamp(tick, 0, damageConfigs.Count - 1);
            return damageConfigs[tick];
        }

        protected void Despawn(bool despawnFx)
        {
            if (_isDeSpawned) return;
            _isDeSpawned = true;
            if (fx && !pools.Contains(fx.gameObject)) // fix bug fx is not disappear, because it is stopped and pooled but then set parent = null
            {
                fx.transform.parent = null;
                fx.transform.localScale = Vector3.one;
                fx.transform.localPosition = Vector3.zero;
                if (fx.isPlaying)
                {
                    fx.Stop();
                }
                if (despawnFx)
                {
                    pools.Despawn(fx.gameObject);
                }
            }
            if (circleObstacleCollection && circleObstacleCollection.Contains(circleObstacle))
            {
                circleObstacleCollection.Remove(circleObstacle);
            }
            if (endAction)
            {
                endAction.Trigger(this, null);
            }
            
            if (scaleAoe)
            {
                if (useDissolveDespawn)
                {
                    DOTween.To(() => 0, x =>
                    {
                        for (var i = 0; i < renderers.Length; i++)
                        {
                            renderers[i].material.SetFloat("_Cutoff", x);
                        }
                    }, 1f, 0.6f).SetTarget(this).Play().OnComplete(() =>
                    {
                        if (isPooling)
                        {
                            pools.Despawn(gameObject);
                        }
                        else
                        {
                            gameObject.SetActive(false);
                        }
                        OnDepawnedEvent?.Invoke();
                    });;
                }
                else
                {
                    DOTween.Kill(this);
                    scaleAoe.DOScale(Vector3.one * 0.2f, 0.3f).SetTarget(this).SetEase(Ease.Linear).Play().OnComplete(() =>
                    {
                        if (isPooling)
                        {
                            pools.Despawn(gameObject);
                        }
                        else
                        {
                            gameObject.SetActive(false);
                        }
                        OnDepawnedEvent?.Invoke();
                    });
                }
            }
            else
            {
                if (isPooling)
                {
                    pools.Despawn(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                OnDepawnedEvent?.Invoke();
            }

        }

        public override void DoDisable()
        {
            base.DoDisable();
            if (circleObstacleCollection && circleObstacleCollection.Contains(circleObstacle))
            {
                circleObstacleCollection.Remove(circleObstacle);
            }
        }

        public ExplodeActionData Get()
        {
            var position = Transform.position;
            return new ExplodeActionData(position, position, 0, SourceData, new DamageConfig(), false);
        }
    }
    
    [Serializable]
    public struct AreaOfEffectData
    {
        [SerializeField] public DamageConfig[] damageData;
        [SerializeField] float tickInterval;
        [SerializeField] float radius;

        public DamageConfig[] DamageData => damageData;
        public float TickInterval => tickInterval;
        public float Radius => radius;

        public void OverrideRadius(float r)
        {
            radius = r;
        }
    }
}