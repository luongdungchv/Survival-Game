using UnityEngine;
using System.Linq;

public class DamagableObject : MonoBehaviour
{
    [SerializeField] protected float hp;
    [SerializeField] protected int priority;
    [SerializeField] protected string[] requiredTools;
    // Start is called before the first frame update

    public virtual void OnDamage(float incomingDmg, int inputPriority, string tool)
    {

    }
}
public interface IDroppableObject
{
    void OnDrop();
}


