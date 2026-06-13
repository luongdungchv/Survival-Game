using System;
using UnityEngine;

namespace BoxHead2.Combat
{
    [Serializable]
    public class HitFeedback
    {
        [SerializeField] Feedback hitWood;
        [SerializeField] Feedback hitMetal;
        [SerializeField] Feedback hitMeat;
        
        public void Play(HitType hitType)
        {
            switch (hitType)
            {
                case HitType.None:
                    if (hitWood)
                    {
                        hitWood.Play();
                    }
                    break;
                case HitType.Wood:
                    if (hitWood)
                    {
                        hitWood.Play();
                    }
                    break;
                case HitType.Metal:
                    if (hitMetal)
                    {
                        hitMetal.Play();
                    }
                    break;
                case HitType.Meat:
                    if (hitMeat)
                    {
                        hitMeat.Play();
                    }
                    break;
                default:
                    if (hitWood)
                    {
                        hitWood.Play();
                    }
                    break;
            }
        }
    }
    
    public enum HitType
    {
        None,
        Wood,
        Metal,
        Meat
    }

    public class HitInfo
    {
        public HitType HitType;
        public float Damage;
    }
}