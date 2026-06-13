using System;
using UnityEngine;

namespace BoxHead2.Items
{
    [Serializable]
    public class DpsElement
    {
        [SerializeField] public float damageMult;
        [SerializeField] public float hit;
        [SerializeField] public float interval;
        [SerializeField] public float duration;
    }
}