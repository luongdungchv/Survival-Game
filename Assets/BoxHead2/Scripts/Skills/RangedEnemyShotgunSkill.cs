using System.Collections;
using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class RangedEnemyShotgunSkill : RangedAutoChargeSkill
    {
        [SerializeField] float delayCancelTrack;
        float Angle => (spreadCount - 1) * spreadAngleStep;
        GameObject _activeIndicator;
        
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

        protected override void DoStop()
        {
            base.DoStop();
            if(_activeIndicator) pools.Despawn(_activeIndicator.gameObject);
        }

        void OnActorDespawn(IActor actor)
        {
            pools.Despawn(_activeIndicator);
            actor.OnDespawnEvent -= OnActorDespawn;
        }

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            if (!_activeIndicator && shootIndicatorPrefab)
            {
                _activeIndicator = pools.Spawn(shootIndicatorPrefab);
                _activeIndicator.gameObject.SetActive(false);
            }

            UseCachedInput = false;
        }

        protected override void UpdateIndicator(Vector3 dir)
        {
            base.UpdateIndicator(dir);
            if (!shootIndicatorPrefab) return;
            if (!_activeIndicator) return;

            _activeIndicator.SetActive(true);

            var pos = weapons[0].LauncherPosition;
            pos.y = Actor.Position.y + 0.01f;
            _activeIndicator.transform.position = pos;
            _activeIndicator.transform.rotation = Actor.Transform.rotation;

            _value += Time.deltaTime / holdDuration;
            _activeIndicator.GetComponent<MaterialModifyIndicator>().UpdateValues(_value);
        }

        public override void OnPrepareShoot(int index)
        {
            base.OnPrepareShoot(index);
            if (_activeIndicator)
            {
                var width = Range * Mathf.Sin(Angle * Mathf.Deg2Rad / 2) * 2;
                _activeIndicator.transform.localScale = new Vector3(width, 1, Range);
            }

            Actor.StartCoroutine(IEDelayCancelTrack());
        }

        IEnumerator IEDelayCancelTrack()
        {
            yield return new WaitForSeconds(delayCancelTrack);
            CacheInput();
            UseCachedInput = true;
            tracking = false;
        }

        public override void OnShoot(int index)
        {
            base.OnShoot(index);
            weapons[0].StopCharge();
            pools.Despawn(_activeIndicator);
            _activeIndicator = null;
        }
    }
}