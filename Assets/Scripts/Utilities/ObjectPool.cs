using System.Collections.Generic;

public class ObjectPool<T> where T : IPoolObject
{
    private Queue<T> pool;
    public ObjectPool()
    {
        pool = new Queue<T>();
    }
    public void AddToPool(T obj)
    {
        obj.OnPooled();
        pool.Enqueue(obj);
    }
    public T Release()
    {
        var obj = pool.Dequeue();
        obj.OnReleased();
        return obj;
    }

}
public interface IPoolObject
{
    object pool { get; set; }
    void OnPooled();
    void OnReleased();
}
