using System.Collections;
using UnityEngine;

namespace BoxHead2.Skills
{
    public class TimeBasedCoordAttackItem : CoordinateAttackItem
    {
        [SerializeField] int triggerCount;
        [SerializeField] float triggerDelay, triggerInterval;

        WaitForSeconds wait;
        
        Coroutine delayCoroutine, damageCoroutine;

        public override void StartDealingDamage()
        {
            wait ??= new WaitForSeconds(triggerInterval);
            delayCoroutine = StartCoroutine(IEDelayDealDamage());
        }

        IEnumerator IEDelayDealDamage()
        {
            yield return new WaitForSeconds(triggerDelay);
            damageCoroutine = StartCoroutine(IEDealDamage());
        }

        IEnumerator IEDealDamage()
        {
            for (int i = 0; i < triggerCount; i++)
            {
                DealDamage(i);
                yield return wait;
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();
            StopAllCoroutines();
        }
    }
}