using BoxHead2.Actor;
using BoxHead2.AnimatorEventCustom;
using BoxHead2.Combat;
using BoxHead2.Helper;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class EvokerTrap : BaseMono
    {
        [SerializeField] HurtBox[] hurtBoxes;
        [SerializeField] ParticleSystem hitFX;
        [SerializeField] Feedback trapTriggerFeedback;
        [SerializeField] string attackAnim;
        [SerializeField] Animator trapAnimator;
        LayerMaskType _damageLayer;

        CombinedDamageData _combinedDamageData;
        Actor.Actor owner;

        AnimatorEventListener _eventListener;
        
        public bool Triggered { get; set; }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void OnDealDamage()
        {
            hitFX.Clear();
            hitFX.Play();
            HurtBoxHelper.ExecuteManualHurtBox(
                transform.position,
                Vector3.up,
                owner.TeamConfig.GetLayerMask(_damageLayer),
                _combinedDamageData,
                false,
                hurtBoxes
            );
            if(trapTriggerFeedback) trapTriggerFeedback.Play();
        }

        public void PlayAttackAnim()
        {
            Triggered = true;
            trapAnimator.Play(attackAnim);
        }

        public void OnEnd()
        {
            pools.Despawn(gameObject);
        }

        public void OnShowSmoke(int index)
        {
        }

        public void SetCombinedDamageData(CombinedDamageData data)
        {
            _combinedDamageData = data;
        }

        public void SetOwner(Actor.Actor owner)
        {
            this.owner = owner;
            var listener = gameObject.GetAndCacheComponentInChildren(ref _eventListener);
            listener.OnBeginAttackEvent += OnDealDamage;
            listener.OnEndAttackEvent += OnEnd;
            listener.OnBeginTrailEvent += OnShowSmoke;
        }

        public void SetSize(float size)
        {
            transform.localScale = new Vector3(size, size, size);
            foreach (var hurtBox in hurtBoxes)
            {
                hurtBox.radius = size;
            }
        }

        public void SetLayer(LayerMaskType damageLayer)
        {
            _damageLayer = damageLayer;
        }

        public override void CleanUp()
        {
            base.CleanUp();
            var listener = gameObject.GetAndCacheComponentInChildren(ref _eventListener);
            listener.OnBeginAttackEvent -= OnDealDamage;
            listener.OnEndAttackEvent -= OnEnd;
            listener.OnStopTrailEvent -= OnShowSmoke;
        }
    }
}