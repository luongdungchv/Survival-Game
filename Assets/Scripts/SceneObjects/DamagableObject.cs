using UnityEngine;
using System.Linq;

public class DamagableObject : MonoBehaviour
{
    [SerializeField] protected string[] requiredTools;
    // Start is called before the first frame update

    public virtual void OnDamage(float incomingDmg, string tool)
    {

    }
    public virtual void OnDamage(IHitData hitData)
    {

    }
}
public interface IDroppableObject
{
    void OnDrop();
}


