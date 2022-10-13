using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Test3 : MonoBehaviour
{
    Collider hitbox;
    Vector3 center;
    Vector3 halfExtents;
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        hitbox = this.GetComponent<Collider>();
        center = hitbox.bounds.center;
        halfExtents = hitbox.bounds.extents;
        Debug.Log(this.GetComponent<Collider>().bounds.size);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var hitbox = this.GetComponent<BoxCollider>();
        center = hitbox.bounds.center;
        halfExtents = hitbox.size / 2;
        center -= transform.right * halfExtents.x;
        halfExtents.x = 0f;
        // RaycastHit hit;
        // if (Physics.BoxCast(center, halfExtents, transform.right, out hit, transform.rotation, 1f, mask))
        // {
        //     Gizmos.DrawWireCube(transform.position + transform.right * hit.distance, hitbox.size);
        //     Gizmos.DrawLine(transform.position, hit.point);
        // }
        var hits = Physics.BoxCastAll(center, halfExtents, transform.right, transform.rotation, 1f, mask);
        if (hits != null)
        {
            foreach (var hit in hits)
            {
                Gizmos.DrawLine(center, hit.point);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("toucj");
    }
}
