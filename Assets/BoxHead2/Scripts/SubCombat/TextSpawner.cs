using System.Collections.Generic;
using BoxHead2.Utils;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    [CreateAssetMenu(menuName = "Sub Combat/Text Spawner")]
    public class TextSpawner : BaseSO, ISerializationCallbackReceiver
    {
        [SerializeField] DamageText playerPhysicalTextPrefab;
        [SerializeField] DamageText physicalTextPrefab;
        [SerializeField] DamageText fireTextPrefab;
        [SerializeField] DamageText iceTextPrefab;
        [SerializeField] DamageText poisonTextPrefab;
        [SerializeField] DamageText electricTextPrefab;
        [SerializeField] DamageText bleedTextPrefab;
        [SerializeField] DamageText criticalTextPrefab;
        [SerializeField] StatusText statusTextPrefab;
        [SerializeField] StatusText healTextPrefab;
        [SerializeField] StatusText skillTextPrefab;
        [SerializeField] StatusText expTextPrefab;

        Queue<StatusText> _statusTextQueue;
        Queue<StatusText> _expTextQueue;
        float _lastTimeFlyStatusText;
        float _lastTimeFlyExpText;
        float _lastTimeFlyDamageText;

        public void SpawnPlayerPhysicText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!playerPhysicalTextPrefab) return;
            SpawnText(playerPhysicalTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnPhysicText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!physicalTextPrefab) return;
            SpawnText(physicalTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnFireText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!fireTextPrefab) return;
            SpawnText(fireTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnIceText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!iceTextPrefab) return;
            SpawnText(iceTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnPoisonText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!poisonTextPrefab) return;
            SpawnText(poisonTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnElectricText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!electricTextPrefab) return;
            SpawnText(electricTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnBleedText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!bleedTextPrefab) return;
            SpawnText(bleedTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnCriticalText(float value, Vector3 pos, Vector3 hitDir)
        {
            if (!criticalTextPrefab) return;
            SpawnText(criticalTextPrefab, value, pos, hitDir);
        }
        
        public void SpawnExpText(double value, Vector3 pos)
        {
            if (!expTextPrefab) return;
            var v = GameConstants.GetFakeExpValue(value);
            if (v == 0) return;
            var text = pools.Spawn(expTextPrefab);
            text.Set( v.ToString(), pos);
            text.gameObject.SetActive(false);
            _expTextQueue.Enqueue(text);
        }

        void SpawnText(DamageText prefab, float value, Vector3 pos, Vector3 hitDir)
        {
            var text = pools.Spawn(prefab);
            text.Set(value, pos, hitDir);
        }

        public void SpawnStatusText(string s, Vector3 pos)
        {
            if (!statusTextPrefab) return;
            var text = pools.Spawn(statusTextPrefab);
            text.Set(s, pos);
            text.gameObject.SetActive(false);
            _statusTextQueue.Enqueue(text);
        }

        public void SpawnHealText(float amount, Vector3 pos)
        {
            if (amount < 1) return;
            var s = Mathf.CeilToInt(amount).ToString();
            var text = pools.Spawn(healTextPrefab);
            text.Set(s, pos);
            text.gameObject.SetActive(false);
            _statusTextQueue.Enqueue(text);
        }
        
        public void SpawnSkillText(string s, Vector3 pos)
        {
            // var text = pools.Spawn(skillTextPrefab);
            // text.Set(s, pos);
            // text.gameObject.SetActive(false);
            // _statusTextQueue.Enqueue(text);
        }

        public override void Tick()
        {
            base.Tick();
            if (_statusTextQueue.Count > 0 && Time.time - _lastTimeFlyStatusText > 0.05f)
            {
                _lastTimeFlyStatusText = Time.time;
                while (_statusTextQueue.Count > 0 && _statusTextQueue.Peek() == null)
                {
                    _statusTextQueue.Dequeue();    
                }
                if (_statusTextQueue.Count > 0)
                {
                    var text = _statusTextQueue.Dequeue();
                    text.gameObject.gameObject.SetActive(true);
                    text.Fly();  
                }
            }
            
            if (_expTextQueue.Count > 0 && Time.time - _lastTimeFlyExpText > 0.05f)
            {
                _lastTimeFlyExpText = Time.time;
                while (_expTextQueue.Count > 0 && _expTextQueue.Peek() == null)
                {
                    _expTextQueue.Dequeue();    
                }
                if (_expTextQueue.Count > 0)
                {
                    var text = _expTextQueue.Dequeue();
                    text.gameObject.gameObject.SetActive(true);
                    text.Fly();  
                }
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _statusTextQueue = new Queue<StatusText>();
            _expTextQueue = new Queue<StatusText>();
            _lastTimeFlyStatusText = 0;
            _lastTimeFlyExpText = 0;
        }
    }
}