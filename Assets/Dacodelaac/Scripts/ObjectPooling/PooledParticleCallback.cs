using System.Collections;
using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.ObjectPooling
{
    public class PooledParticleCallback : BaseMono
    {
        void OnParticleSystemStopped()
        {
            StartCoroutine(IEDespawn());
        }

        IEnumerator IEDespawn()
        {
            yield return null;
            pools.Despawn(gameObject);
        }
    }
}