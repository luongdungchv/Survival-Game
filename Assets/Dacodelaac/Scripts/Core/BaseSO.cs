using System;
using System.Linq;
using System.Reflection;
using BoxHead2.Utils;
using Dacodelaac.ObjectPooling;
using Dacodelaac.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Core
{
    public class BaseSO : ScriptableObject, IEntity
    {
        [SerializeField] string id;
        [Header("Base")]
        [SerializeField] public Pools pools;
        [SerializeField] public Ticker ticker;
        [SerializeField] bool earlyTick;
        [SerializeField] bool tick;
        [SerializeField] bool lateTick;
        [SerializeField] bool fixedTick;

        public string Id => id;
        BaseSO original;
        public bool IsInstanceOf(BaseSO o) => original != null && original == o;
        public bool IsSameOriginal(BaseSO o) => original != null && original == o.original;
        
        public BaseSO GetOriginal()
        {
            return original;
        }
        
#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        [ContextMenu("Reset Id")]
        public void ResetId()
        {
            id = name;
            EditorUtility.SetDirty(this);
        }

        [Sirenix.OdinInspector.Button]
        [ContextMenu("Set null ID")]
        public void SetNullID()
        {
            id = "_";
            EditorUtility.SetDirty(this);
        }
        
#endif
        
        public virtual T CreateCopy<T>() where T : ScriptableObject
        {
            var copy = Instantiate(this);
            copy.original = original ? original : this;
            return copy as T;
        }

        public void Enable()
        {
            BindVariable();
            ListenEvents();
            SubTick();
            DoEnable();
        }

        public void Disable()
        {
            DoDisable();
            UnsubTick();
            StopListenEvents();
            UnbindVariable();
        }

        public virtual void BindVariable()
        {
        }

        public virtual void ListenEvents()
        {
        }

        public virtual void DoEnable()
        {
        }

        void SubTick()
        {
            if (earlyTick) ticker.SubEarlyTick(this);
            if (tick) ticker.SubTick(this);
            if (lateTick) ticker.SubLateTick(this);
            if (fixedTick) ticker.SubFixedTick(this);
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