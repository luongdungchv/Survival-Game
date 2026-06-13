using System;
using BoxHead2.Actor;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    [Serializable]
    public class DisplayText
    {
        [SerializeField] string text;

        public bool ShouldDisplay => !string.IsNullOrEmpty(text);

        public void Display(IActor actor)
        {
            if (actor != null)
            {
                actor.SpawnSkillText(text, actor.LockPosition);
            }
        }
    }
}