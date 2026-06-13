using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Helper;
using UnityEngine;

namespace BoxHead2.Items
{
    public class RangedWeapon : Weapon
    {
        [SerializeField] Transform localLauncher;
        [SerializeField] Transform realLauncher;
        [SerializeField] GameObject launcherPrefab;
        [SerializeField] GameObject bulletPlaceholder;
        [SerializeField] bool showBulletPlaceholderOnIdle;
        [SerializeField] ParticleSystem muzzleFlashPrefab;
        [SerializeField] ParticleSystem muzzleFlash;
        [SerializeField] ParticleSystem loopMuzzleFlash;
        [SerializeField] Feedback loopFeedback;
        [SerializeField] Transform bulletEjectAnchor;
        [SerializeField] ParticleSystem bulletEjectFx;
        [SerializeField] ParticleSystem chargeFullFx;
        [SerializeField] LineRenderer aimLinePrefab;

        public override Vector3 LauncherPosition => localLauncher.position;
        public Vector3 LauncherForward => localLauncher.forward;

        public Vector3 RealLauncherPosition => realLauncher ? realLauncher.position : LauncherPosition;

        List<LineRenderer> aimLines;

        public override void Initialize()
        {
            base.Initialize();
            ToggleBulletPlaceHolder(showBulletPlaceholderOnIdle);
            StopShootLoopFx();
            ClearAimLine();
        }

        public override void OnAttach(IEquipmentAttachable owner, WeaponAttachSlot slot, bool onBodyWeapon)
        {
            base.OnAttach(owner, slot, onBodyWeapon);
            if (launcherPrefab)
            {
                if (localLauncher)
                {
                    Destroy(localLauncher.gameObject);
                    localLauncher = null;
                }
                localLauncher = Instantiate(launcherPrefab, owner.Transform).transform;
            }
            ClearAimLine();
        }

        public override void OnDetach()
        {
            base.OnDetach();
            StopShootLoopFx();
            if (launcherPrefab && localLauncher)
            {
                Destroy(localLauncher.gameObject);
            }
            ClearAimLine();
        }

        public override void PrepareShoot()
        {
            ToggleBulletPlaceHolder(true);
        }

        public override void Shoot()
        {
            ToggleBulletPlaceHolder(false);
            PlayShootFx();
            ClearAimLine();
        }

        void PlayShootFx()
        {
            if (muzzleFlashPrefab)
            {
                var fx = pools.Spawn(muzzleFlashPrefab);
                var t = fx.transform;
                t.position = localLauncher.position;
                t.rotation = localLauncher.rotation;
                fx.Play();
            }
            if (muzzleFlash)
            {
                WeaponVisualSetter.Bind(Owner as IActor, muzzleFlash.gameObject);
                muzzleFlash.Play();
            }
            if (bulletEjectFx)
            {
                var fx = pools.Spawn(bulletEjectFx);
                var t = fx.transform;
                t.position = bulletEjectAnchor.position;
                t.rotation = bulletEjectAnchor.rotation;
                fx.Play();
            }
        }

        public void ToggleBulletPlaceHolder(bool on)
        {
            if (bulletPlaceholder)
            {
                bulletPlaceholder.gameObject.SetActive(on);
            }
        }

        public override void OnStopUse()
        {
            base.OnStopUse();
            ClearAimLine();
            StopCharge();
            ToggleBulletPlaceHolder(showBulletPlaceholderOnIdle);
            if (muzzleFlash)
            {
                muzzleFlash.Stop();
            }
        }

        public override void PlayShootLoopFx(float aliveTime)
        {
            base.PlayShootLoopFx(aliveTime);
            if (loopMuzzleFlash)
            {
                if (aliveTime > 0)
                {
                    var main = loopMuzzleFlash.main;
                    main.startLifetime = aliveTime;
                }
                if (loopMuzzleFlash.isPlaying)
                {
                    loopMuzzleFlash.Stop();
                }
                loopMuzzleFlash.Play();
            }
        }

        public override void StopShootLoopFx()
        {
            base.StopShootLoopFx();
            if (loopMuzzleFlash && loopMuzzleFlash.isPlaying)
            {
                loopMuzzleFlash.Stop();
            }
        }

        public override void OnChargeFull()
        {
            if (chargeFullFx)
            {
                if (chargeFullFx.isPlaying)
                {
                    chargeFullFx.Stop();
                }
                chargeFullFx.Clear();
                chargeFullFx.Play();
            }
        }

        void ClearAimLine()
        {
            if (aimLines == null)
            {
                aimLines = new List<LineRenderer>();
            }
            foreach (var line in aimLines)
            {
                pools.Despawn(line.gameObject);
            }
            aimLines.Clear();
        }

        public override void ShowIndicator(Vector3 aimDir, Vector3 aimPos, float range, int spreadCount, float spreadAngleStep)
        {
            if (!aimLinePrefab) return;
            
            while (aimLines.Count < spreadCount)
            {
                var aimLine = pools.Spawn(aimLinePrefab);
                aimLines.Add(aimLine);
            }
            while (aimLines.Count > spreadCount)
            {
                pools.Despawn(aimLines[aimLines.Count - 1].gameObject);
                aimLines.RemoveAt(aimLines.Count - 1);
            }

            aimDir.y = 0;
            aimDir.Normalize();
            range = SkillHelper.CalculateShootRange(RealLauncherPosition, Owner.Transform.position, aimDir, range);
            var angle = -spreadAngleStep * (spreadCount - 1) * 0.5f;
            
            for (var i = 0; i < aimLines.Count; i++)
            {
                var direction = Quaternion.Euler(0, angle, 0) * aimDir;
                
                aimLines[i].positionCount = 2;
                aimLines[i].SetPosition(0, RealLauncherPosition);
                aimLines[i].SetPosition(1, RealLauncherPosition + direction * range);
                
                angle += spreadAngleStep;
            }
        }
    }
}