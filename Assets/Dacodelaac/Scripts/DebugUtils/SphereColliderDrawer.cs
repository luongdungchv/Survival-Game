using System;
using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.DebugUtils
{
    public class SphereColliderDrawer : BaseMono
    {
        [SerializeField] Color color = Color.blue;
        
        void OnDrawGizmos()
        {
            var sphere = GetComponent<SphereCollider>();
            if (sphere)
            {
                Gizmos.color = color;
                Gizmos.DrawWireSphere(Transform.position + sphere.center, Transform.localScale.x * sphere.radius);
            }
        }
    }
}