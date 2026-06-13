using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.Items
{
    public class WeaponAttachSlot : BaseMono
    {
        [SerializeField] public WeaponAttachSlotType slotType;
        [SerializeField] bool flatRotation;

        Transform _target;

        public void Bind(Transform target)
        {
            _target = target;
        }

        public void UnBind()
        {
            _target = null;
        }

        public override void DoEnable()
        {
            base.DoEnable();
            Follow();
        }

        public override void Tick()
        {
            Follow();
        }

        void Follow()
        {
            if (_target == null) return;
            transform.position = _target.position;
            var rot = _target.rotation;
            if (flatRotation)
            {
                var forward = _target.forward;
                forward.y = 0;
                rot = Quaternion.LookRotation(forward);
            }
            transform.rotation = rot;
            transform.localScale = _target.localScale;
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
    
    public enum WeaponAttachSlotType
    {
        None,
        Sword,
        Bow,
        Staff,
        Axe,
        Dagger,
        Sickle,
        StaffUnique,
        KungfuWeapon,
        Rapier,
        Gun
    }
}