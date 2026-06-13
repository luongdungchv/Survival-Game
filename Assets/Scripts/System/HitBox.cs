using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HitBox : MonoBehaviour
{
    protected BoxCollider hitbox;
    protected ParticleSystem atkVfx;
    [SerializeField] protected ParticleSystem hitVfx;
    [SerializeField] private LayerMask mask;
    [SerializeField] private bool logDetect;

    private RaycastHit[] hitBuffer;
    
    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        atkVfx = GetComponentInChildren<ParticleSystem>();
        
        hitBuffer = new RaycastHit[10];

    }

    public void DetectHit()
    {
        var worldScale = transform.lossyScale;
        var hitboxWorldSize = new Vector3(hitbox.size.x * worldScale.x, hitbox.size.y * worldScale.y, hitbox.size.z * worldScale.z);
        var halfExtents = hitboxWorldSize / 2;
        var origin = hitbox.bounds.center - transform.right * halfExtents.x;
        var size = hitboxWorldSize.x;
        halfExtents.x = 0;
        atkVfx?.Play();
        if (this.logDetect) Debug.LogError(1);
        var count = Physics.BoxCastNonAlloc(origin, halfExtents, transform.right, this.hitBuffer, transform.rotation, size, mask);
        if (count > 0)
        {
            var canBreak = false;
            for(int i = 0; i < count; i++)
            {
                var hit = hitBuffer[i];
                Debug.LogError((2, hit.collider.name));
                if (OnHitDetect(hit)) canBreak = true;
            }
            if (canBreak) return;
        }
        if (this.logDetect) Debug.LogError(3);
        OnNoHitDetect();
    }
    protected virtual bool OnHitDetect(RaycastHit hit)
    {
        return false;
    }
    protected virtual void OnNoHitDetect()
    {

    }
    private void OnDestroy()
    {
        OnNoHitDetect();
    }
}
public interface IHitData
{

}
public class PlayerHitData : IHitData
{
    public float damage;
    public string atkTool;
    public PlayerStats dealer;
    public bool crit;
    public bool knockback;
    public PlayerHitData(float damage, string atkTool, PlayerStats dealer, bool crit, bool knockback = false)
    {
        this.damage = damage;
        this.atkTool = atkTool;
        this.dealer = dealer;
        this.crit = crit;
        this.knockback = knockback;
    }
}
