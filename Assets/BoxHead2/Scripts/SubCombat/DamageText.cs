using Dacodelaac.Core;
using Dacodelaac.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    public class DamageText : BaseMono
    {
        [SerializeField] TextMeshPro text;
        [SerializeField, Multiline] string format = "{0}";

        public override void Initialize()
        {
            transform.localScale = Vector3.one;
            text.alpha = 1;
        }

        public void Set(float damage, Vector3 pos, Vector3 hitDir)
        {
            text.text = string.Format(format, damage);
            Fly(pos, hitDir);
        }

        void Fly(Vector3 pos, Vector3 hitDir)
        {
            var targetPos = pos;
            if (hitDir.x > 0)
            {
                targetPos += Vector3.right;
            }
            else
            {
                targetPos += Vector3.left;
            }
            transform.position = pos;
            transform.DOJump(targetPos, 2, 1, 1f).SetTarget(this).Play();
            text.DOFade(0, 0.3f).SetDelay(0.5f).SetTarget(this).Play();
            transform.DOScale(0, 0.5f).SetDelay(0.5f).SetTarget(this).OnComplete(() =>
            {
                DOTween.Kill(this);
                pools.Despawn(gameObject);
            }).Play();
        }

        public override void CleanUp()
        {
            base.CleanUp();
            DOTween.Kill(this);
        }
    }
}