using System.Collections;
using System.Linq;
using BoxHead2.SubActions;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class EnemyHealSkill : Skill
    {
        [SerializeField] string startAnimation, endAnimation;
        [SerializeField] AnimationClip clip;
        [SerializeField] float animSpeed = 1;
        [SerializeField] HealType healType;
        [SerializeField] float healAmount;
        [SerializeField] float healDuration;
        [SerializeField] ParticleSystem fxHealPrefab;
        [SerializeField] Feedback healFeedback;
        public override bool IsStopConditionMet => !Actor.IsPlayingAnim;

        bool _healing;
        HealData _healData;
        float _trueHealAmount;

        ParticleSystem _activeHealFX;

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            _healData = new HealData
            {
                Amount = 0,
                ActiveFx = false,
                HpText = false,
                Revive = false,
                ReviveText = false,
            };

            switch (healType)
            {
                case HealType.Flat:
                    _trueHealAmount = healAmount;
                    break;
                case HealType.Percent:
                    var hp = Actor.MaxHp;
                    _trueHealAmount = hp * healAmount;
                    break;
            }

            if (fxHealPrefab)
            {
                _activeHealFX = pools.Spawn(fxHealPrefab);
                _activeHealFX.transform.SetParent(Actor.Transform);
                _activeHealFX.transform.localPosition = fxHealPrefab.transform.localPosition;
                _activeHealFX.transform.localScale = fxHealPrefab.transform.localScale;
            }
        }

        protected override void DoStop()
        {
            base.DoStop();
            if(_activeHealFX) pools.Despawn(_activeHealFX.gameObject);
        }

        protected override IEnumerator IEPerform()
        {
            Actor.PlayAnimation(startAnimation, 0, animSpeed, false, false);
            var healTimeElapsed = 0f;
            
            yield return new WaitUntil(() => _healing);

            while (healTimeElapsed < healDuration)
            {
                _healData.Amount = _trueHealAmount * Time.deltaTime / healDuration;
                Actor.Heal(_healData);
                healTimeElapsed += Time.deltaTime;
                yield return null;
            }

            if (healTimeElapsed >= healDuration)
            {
                Actor.PlayAnimation(endAnimation, 0, animSpeed, false, false);
                _activeHealFX?.Stop();
                healFeedback?.Stop();
            }
        }

        public override void OnBeginMove(int index)
        {
            base.OnBeginMove(index);
            _activeHealFX?.Play();
            healFeedback?.Play();
            _healing = true;
        }

        enum HealType
        {
            Flat, Percent
        }
    }
}