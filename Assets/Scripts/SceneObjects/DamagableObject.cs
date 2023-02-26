using UnityEngine;
using System.Linq;

public class DamagableObject : MonoBehaviour
{
    [SerializeField] protected string[] requiredTools;

    public virtual void OnDamage(float incomingDmg, string tool)
    {

    }
    public virtual void OnDamage(IHitData hitData)
    {

    }
}
public interface IDamagable
{
    void OnDamage(IHitData hitData);
}


