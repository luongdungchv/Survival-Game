﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    protected BoxCollider hitbox;
    protected ParticleSystem atkVfx;
    [SerializeField] protected ParticleSystem hitVfx;
    [SerializeField] private LayerMask mask;
    // Start is called before the first frame update
    private void Awake()
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
            var canBreak = false;
            foreach (var hit in hits)
                if (OnHitDetect(hit)) canBreak = true;
            if (canBreak) return;
        }
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
    public PlayerHitData(float damage, string atkTool, PlayerStats dealer, bool crit)
    {
        this.damage = damage;
        this.atkTool = atkTool;
        this.dealer = dealer;
        this.crit = crit;
    }
}
