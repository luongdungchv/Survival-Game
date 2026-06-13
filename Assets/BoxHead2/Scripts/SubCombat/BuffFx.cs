using System;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    [Serializable]
    public class BuffFx
    {
        [SerializeField] BuffType type;
        [SerializeField] ParticleSystem[] buffFxs;

        public BuffType Type => type;

        public void Play()
        {
            foreach (var fx in buffFxs)
            {
                if (!fx.isPlaying)
                {
                    fx.Play();
                }
            }
        }
        
        public void Stop()
        {
            foreach (var fx in buffFxs)
            {
                if (fx.isPlaying)
                {
                    fx.Stop();
                }
            }
        }

        public void ResetFx()
        {
            Stop();
        }
    }
}