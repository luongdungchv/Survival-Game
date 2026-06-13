using System.Collections;
using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Items;
using BoxHead2.SubCombat;
using Dacodelaac.ObjectPooling;
using Dacodelaac.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace BoxHead2.Helper
{
    public static class FxHelper
    {
        public static void SpawnExplodeFx(Pools pools, Vector3 pos, Vector3 dir, float radius, bool scaleFx,
            ParticleSystem explodeFxPrefab, Feedback explodeFeedback, IActor actor)
        {
            if (explodeFxPrefab)
            {
                var fx = pools.Spawn(explodeFxPrefab);
                fx.transform.position = pos;
                if (dir != Vector3.zero)
                {
                    fx.transform.rotation = Quaternion.LookRotation(dir);
                }

                fx.transform.localScale = scaleFx ? Vector3.one * radius : explodeFxPrefab.transform.localScale;
                WeaponVisualSetter.Bind(actor, fx.gameObject);
                fx.GetComponent<ExplosionParticle>()?.Setup();
                fx.Play();
            }
            if (explodeFeedback)
            {
                explodeFeedback.Play();
            }
        }
        
        public static void SpawnFx(Pools pools, Vector3 pos, Vector3 dir, ParticleSystem fxPrefab, Feedback feedback)
        {
            if (fxPrefab)
            {
                var fx = pools.Spawn(fxPrefab);
                fx.transform.position = pos;
                fx.transform.rotation = dir == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(dir);
                fx.transform.localScale = fxPrefab.transform.localScale;
                fx.GetComponent<ExplosionParticle>()?.Setup();
                fx.Play();
            }
            if (feedback)
            {
                feedback.Play();
            }
        }
        
        public static void SpawnFxAttachTransform(Pools pools, Transform transform, ParticleSystem fxPrefab, Feedback feedback)
        {
            if (fxPrefab)
            {
                var fx = pools.Spawn(fxPrefab);
                fx.transform.parent = transform;
                fx.transform.localPosition = Vector3.zero;
                fx.transform.rotation = Quaternion.identity;
                fx.transform.localScale = fxPrefab.transform.localScale;
                fx.GetComponent<ExplosionParticle>()?.Setup();
                fx.Play();
            }
            if (feedback)
            {
                feedback.Play();
            }
        }
        
        // public static PickupItem SpawnPickupSingle(Pools pools, Vector3 pos, PickupItem pickupItemPrefab, Feedback feedback, bool anim)
        // {
        //     // pos.y = 0;
        //     var item = pools.Spawn(pickupItemPrefab);
        //     var t = item.transform;
        //     t.position = pos;
        //     t.rotation = Quaternion.identity;
        //     if (anim)
        //     {
        //         item.transform.DOMoveY(10, 0.3f).From().SetEase(Ease.InQuad).OnComplete(() =>
        //         {
        //             item.OnGrounded();
        //         }).Play();
        //     }
        //     else
        //     {
        //         item.OnGrounded();
        //     }
        //     if (feedback)
        //     {
        //         feedback.Play();
        //     }
        //
        //     return item;
        // }
        
        
        // public static void SpawnEnemyPickupMulti(Pools pools, Vector3 pos, float value, float speed, float delayTime, EnemyPickupItem pickupItemPrefab, Feedback feedback, AIEnemy enemy)
        // {
        //     var count = Mathf.Max(1, Mathf.RoundToInt(value));
        //     for (var i = 0; i < count; i++)
        //     {
        //         var targetPos = pos + SimpleMath.RandomInCircleXZ() * 5;
        //         targetPos.y = 0;
        //         
        //         var item = pools.Spawn(pickupItemPrefab);
        //         item.speed = speed;
        //         item.delayTime = delayTime;
        //         item.transform.position = pos;
        //         item.transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);
        //         item.transform.DOJump(targetPos, Random.Range(1f, 3f), Random.Range(1, 3), Random.Range(0.5f, 1f)).SetEase(Ease.Linear).OnComplete(() =>
        //         {
        //             item.OnGrounded(enemy);
        //         }).Play();
        //     }
        //     if (feedback)
        //     {
        //         feedback.Play();
        //     }
        // }
    }
}