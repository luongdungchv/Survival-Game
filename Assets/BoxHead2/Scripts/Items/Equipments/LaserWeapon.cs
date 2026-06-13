using UnityEngine;

namespace BoxHead2.Items
{
    public class LaserWeapon : RangedWeapon
    {
        [Header("Specific Props")]
        [SerializeField] float normalLaserDistance;
        [SerializeField] ParticleSystem hitParticle;
        [SerializeField] Transform laserLauncher;
        [SerializeField] ParticleSystem loopParticle;
        [SerializeField] Feedback shootFeedback;
        [SerializeField] bool playHitFXAtLaserEndPoint;
        bool _isShoot;

        public override void Initialize()
        {
            base.Initialize();
            laserLauncher.gameObject.SetActive(false);
            hitParticle.Stop();
            loopParticle.Stop();
        }

        public override void Shoot()
        {
            _isShoot = true;
            StopCharge();
            if (loopParticle)
            {
                loopParticle.Play();
            }

            if (shootFeedback)
            {
                shootFeedback.Play();
            }
            laserLauncher.gameObject.SetActive(true);
            base.Shoot();
        }
        
        public void UpdateLaserDistance(float distance, bool isHit, Vector3 hitPoint)
        {
            if (!_isShoot) return;
            if (hitParticle)
            {
                if (isHit)
                {
                    if (hitParticle.isStopped)
                    {
                        hitParticle.Play();
                    }

                    hitParticle.transform.position = hitPoint;
                }
                else if (!isHit)
                {
                    if(!playHitFXAtLaserEndPoint && hitParticle.isPlaying)
                        hitParticle.Stop();
                    else
                    {
                        hitParticle.transform.position = hitPoint;
                        if (hitParticle.isStopped) hitParticle.Play();
                    }
                }
            }
            
            if (distance > 0)
            {
                var a = Vector3.one;
                a.z *= distance / normalLaserDistance;
                laserLauncher.localScale = a;
            }
            else
            {
                var a = Vector3.one;
                a.z *= 30;
                laserLauncher.localScale = a;
            }
        }

        public override void OnStopUse()
        {
            _isShoot = false;
            laserLauncher.gameObject.SetActive(false);
            if (hitParticle) hitParticle.Stop();
            if (loopParticle) loopParticle.Stop();
            if (shootFeedback) shootFeedback.Stop();
            base.OnStopUse();
        }
    }
}