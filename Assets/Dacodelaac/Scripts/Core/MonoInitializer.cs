using System;

namespace Dacodelaac.Core
{
    public class MonoInitializer : BaseMono
    {
        void Start()
        {
            pools.Initialize(gameObject);
        }
    }
}