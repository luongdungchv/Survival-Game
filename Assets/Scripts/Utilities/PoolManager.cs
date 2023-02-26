using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager ins;
    public ObjectPool<DamagePopup> dmgPopupPool;
    private void Awake()
    {
        ins = this;
        dmgPopupPool = new ObjectPool<DamagePopup>();
    }
}
