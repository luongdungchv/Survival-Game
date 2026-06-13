using Dacodelaac.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    public class StatusText : BaseMono
    {
        [SerializeField] TextMeshPro text;
        [SerializeField] float flyHeight = 2f;
        [SerializeField, Multiline] string format = "{0}";
        [SerializeField] float flyTime = 0.8f;

        Vector3 _pos;

        public override void Initialize()
        {
            transform.localScale = Vector3.one;
            text.alpha = 1;
        }

        public void Set(string status, Vector3 pos)
        {
            text.text = string.Format(format, status);
            _pos = pos;
        }

        public void Fly()
        {
            transform.position = _pos;
            transform.DOMoveY(_pos.y + flyHeight, flyTime).SetTarget(this).Play();
            text.DOFade(0.2f, flyTime - 0.5f).SetDelay(0.5f).SetTarget(this).OnComplete(() =>
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