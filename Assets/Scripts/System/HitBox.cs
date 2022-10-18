using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    private BoxCollider hitbox;
    private ParticleSystem atkVfx;
    [SerializeField] private LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<BoxCollider>();
        atkVfx = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void DetectHit()
    {

        var halfExtents = hitbox.size / 2;
        var origin = hitbox.bounds.center - transform.right * halfExtents.x;
        var size = hitbox.size.x;
        halfExtents.x = 0;
        atkVfx.Play();

        var hits = Physics.BoxCastAll(origin, halfExtents, transform.right, transform.rotation, size, mask);
        if (hits != null && hits.Length > 0)
        {
            foreach (var hit in hits)
            {

                //Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.TryGetComponent<DamagableObject>(out var target))
                {
                    Debug.Log($"{halfExtents} {origin} {size} ");
                    target.OnDamage(25, 3, "test");
                }
            }
        }
    }

}
