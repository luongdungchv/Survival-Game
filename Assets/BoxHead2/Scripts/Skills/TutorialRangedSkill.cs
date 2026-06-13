using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Event = Dacodelaac.Events.Event;

namespace BoxHead2.Skills
{
    public class TutorialRangedSkill : RangedSkill
    {
        [SerializeField] Event triggerEvent;
        [SerializeField] float sqrRange;
        public override void OnShoot(int index)
        {
            base.OnShoot(index);
        }

        
    }
}