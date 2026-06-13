using System;
using System.Linq;
using System.Reflection;
using Dacodelaac.ObjectPooling;
using Dacodelaac.Utils;
#if UNITY_EDITOR
using BoxHead2.Utils;
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Core
{
    public abstract class BaseMono : MonoBehaviour, IEntity
    {
        [Header("Base")]
        [SerializeField] public Identity identity;
        [SerializeField] public Pools pools;
        [SerializeField] public Ticker ticker;
        [SerializeField] protected bool initOnStart;
        [SerializeField] bool earlyTick;
        [SerializeField] bool tick;
        [SerializeField] bool lateTick;
        [SerializeField] bool fixedTick;
        [SerializeField] bool cleanUpOnDestroy;
        
        Transform mTransform;
        public Transform Transform => gameObject.GetAndCacheComponent(ref mTransform);

        void OnEnable()
        {
            BindVariable();
            ListenEvents();
            SubTick();
            DoEnable();
        }

        void Start()
        {
            if (initOnStart)
            {
                Initialize();
            }
        }

        void OnDisable()
        {
            DoDisable();
            UnsubTick();
            StopListenEvents();
            UnbindVariable();
        }

        public virtual void BindVariable()
        {
            BindIdentity();
        }
        
        protected void BindIdentity()
        {
            if (identity)
            {
                if (identity.Value == null)
                {
                    identity.Value = this;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError($"Cannot bind {name} to identity {identity.name}, currently is {identity.Value}");
#endif
                }
            }
        }

        public virtual void ListenEvents()
        {
        }
        
        void SubTick()
        {
            if (earlyTick) ticker.SubEarlyTick(this);
            if (tick) ticker.SubTick(this);
            if (lateTick) ticker.SubLateTick(this);
            if (fixedTick) ticker.SubFixedTick(this);
        }

        public virtual void DoEnable()
        {
        }

        public virtual void Initialize()
        {
        }
        
        public virtual void EarlyTick()
        {
        }

        public virtual void Tick()
        {
        }

        public virtual void LateTick()
        {
        }

        public virtual void FixedTick()
        {
        }
        
        public virtual void CleanUp()
        {
        }
        
        public virtual void DoDisable()
        {
        }

        void UnsubTick()
        {
            if (earlyTick) ticker.UnsubEarlyTick(this);
            if (tick) ticker.UnsubTick(this);
            if (lateTick) ticker.UnsubLateTick(this);
            if (fixedTick) ticker.UnsubFixedTick(this);
        }

        public virtual void StopListenEvents()
        {
        }

        public virtual void UnbindVariable()
        {
            UnbindIdentity();
        }

        protected void UnbindIdentity()
        {
            if (identity && identity.Value != null)
            {
                if (identity.Value == this)
                {
                    identity.Value = null;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError($"Cannot unbind {name} from identity {identity.name}");
#endif
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (!cleanUpOnDestroy) return;
            OnDisable();
            CleanUp();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.pools == null)
            {
                var pools = AssetUtils.FindAssetAtFolder<Pools>("Assets");
                if(pools != null && pools.Length > 0) this.pools = pools[0];
            }

            if (this.ticker == null)
            {
                var tickers = AssetUtils.FindAssetAtFolder<Ticker>("Assets");
                if(tickers != null && tickers.Length > 0) this.ticker = tickers[0];
            }
        }
#endif
    }
}