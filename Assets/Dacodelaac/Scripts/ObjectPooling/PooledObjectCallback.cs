using Dacodelaac.Core;

namespace Dacodelaac.ObjectPooling
{
    public class PooledObjectCallback :BaseMono
    {
        public void Despawn()
        {
            pools.Despawn(gameObject);
        }
    }
}