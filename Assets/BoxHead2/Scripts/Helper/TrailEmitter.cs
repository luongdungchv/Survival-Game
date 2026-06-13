using System;
using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Helper
{
    public class TrailEmitter : BaseMono
    {
        [SerializeField] ParticleSystem trailFxPrefab;

        ParticleSystem fx;
        Transform fxTransform;
        FxState state;

        public override void DoEnable()
        {
            base.DoEnable();
            state = FxState.Wait;
        }

        public override void DoDisable()
        {
            base.DoDisable();
            if (state != FxState.Stop)
            {
                if (fx != null)
                {
                    fx.Stop();
                }
                state = FxState.Stop;
            }
        }

        public override void Tick()
        {
            switch (state)
            {
                case FxState.Wait:
                    fx = pools.Spawn(trailFxPrefab);
                    fxTransform = fx.transform;
                    state = FxState.Spawn;
                    Follow();
                    break;
                case FxState.Spawn:
                    fx.Clear(true);
                    fx.Play();
                    Follow();
                    state = FxState.Play;
                    break;
                case FxState.Play:
                    Follow();
                    break;
                case FxState.Stop:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void Follow()
        {
            if (fx != null)
            {
                fxTransform.position = Transform.position;
                fxTransform.rotation = Transform.rotation;
            }
        }
    }

    enum FxState
    {
        Wait,
        Spawn,
        Play,
        Stop
    }
}