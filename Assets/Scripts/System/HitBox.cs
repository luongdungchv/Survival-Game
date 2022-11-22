using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    protected BoxCollider hitbox;
    protected ParticleSystem atkVfx;
    [SerializeField] protected ParticleSystem hitVfx;
    [SerializeField] private LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<BoxCollider>();
        atkVfx = GetComponentInChildren<ParticleSystem>();

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
        var hits = Physics.BoxCastAll(origin, halfExtents, transform.right, transform.rotation, size, mask);
        if (hits != null && hits.Length > 0)
        {

            foreach (var hit in hits)
            {
                OnHitDetect(hit);
            }
            return;
        }
        OnNoHitDetect();
    }
    protected virtual void OnHitDetect(RaycastHit hit)
    {

    }
    protected virtual void OnNoHitDetect()
    {

    }
}
public interface IHitData
{

}
public class PlayerHitData : IHitData
{
    public float damage;
    public string atkTool;
    public HitBox dealer;
    public PlayerHitData(float damage, string atkTool, HitBox dealer)
    {
        this.damage = damage;
        this.atkTool = atkTool;
        this.dealer = dealer;
    }
}
