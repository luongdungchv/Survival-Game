using BoxHead2.Actor;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Items
{
    public class Weapon : BaseMono
    {
        [SerializeField] public GameObject visual;
        [SerializeField] ParticleSystem chargeFx;
        [SerializeField] ParticleSystem stopChargeFx;
        
        public WeaponData Data { get; private set; }
        public IEquipmentAttachable Owner { get; private set; }
        
        public virtual Vector3 LauncherPosition => Transform.position;

        public void Setup(WeaponData data)
        {
            Data = data;
        }
        
        public virtual void OnAttach(IEquipmentAttachable owner, WeaponAttachSlot slot, bool onBodyWeapon)
        {
            Owner = owner;
            if (!onBodyWeapon)
            {
                Transform.parent = slot.transform;
                Transform.ResetLocal();
            }
            
            WeaponVisualSetter.Bind(owner as IActor, gameObject);
            if (Data is {IsRangedWeapon : false} || !onBodyWeapon) Show();
            else Hide();
        }
        
        
        public virtual void OnDetach()
        {
        }

        public void ToggleTrail(bool on)
        {
        }

        public virtual void PrepareShoot()
        {
            
        }
        
        public virtual void Shoot()
        {
            
        }

        public virtual void OnStopUse()
        {
        }

        public void Hide()
        {
            if (visual)
            {
                visual.SetActive(false);
            }
        }

        public void Show()
        {
            if (visual)
            {
                visual.SetActive(true);
            }
        }
        
        public virtual void PlayShootLoopFx(float aliveTime)
        {
        }
        public virtual void StopShootLoopFx()
        {
        }

        public virtual void ShowIndicator(Vector3 aimDir, Vector3 aimPos, float range, int spreadCount, float spreadAngleStep)
        {
        }

        public virtual void StartCharge()
        {
            if (stopChargeFx)
            {
                stopChargeFx.Stop();
                stopChargeFx.Clear();
            }

            if (chargeFx)
            {
                chargeFx.Clear();
                chargeFx.Play();
            }
        }
        
        public virtual void OnChargeFull()
        {
        }

        public virtual void StopCharge()
        {
            if (chargeFx)
            {
                chargeFx.Stop();
            }
            
            if (stopChargeFx)
            {
                stopChargeFx.Play();
            }
        }
    }
}