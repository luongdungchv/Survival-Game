using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using Dacodelaac.Collections;
using Dacodelaac.Utils;
using UnityEngine;

namespace BoxHead2.Collection
{
    [CreateAssetMenu(menuName = "Collections/IDamageTaker")]
    public class DamageTakerCollection : BaseCollection<IDamageTaker>
    {
        public IDamageTaker GetNearest(out float minSqrRange, Vector3 pos, float range, HashSet<IDamageTaker> exclude)
        {
            IDamageTaker result = null;
            minSqrRange = range * range;
            for (var i = Count - 1; i >= 0; i--)
            {
                if (this[i].CanBeTarget && (exclude == null || !exclude.Contains(this[i])))
                {
                    var ran = SimpleMath.SqrDistXZ(this[i].Position, pos);
                    if (ran <= minSqrRange)
                    {
                        result = this[i];
                        minSqrRange = ran;
                    }
                }
            }

            return result;
        }
        
        public IDamageTaker GetFarthest(Vector3 pos, float range, HashSet<IDamageTaker> exclude)
        {
            IDamageTaker result = null;
            var minRange = 0f;
            for (var i = Count - 1; i >= 0; i--)
            {
                if (this[i].CanBeTarget && (exclude == null || !exclude.Contains(this[i])))
                {
                    var ran = SimpleMath.SqrDistXZ(this[i].Position, pos);
                    if (ran <= range * range && minRange < ran)
                    {
                        result = this[i];
                        minRange = ran;
                    }
                }
            }

            return result;
        }

        public IDamageTaker GetRandom(Vector3 pos, float range, HashSet<IDamageTaker> exclude)
        {
            var results = new List<IDamageTaker>();
            var sqrRange = range * range;
            for (var i = Count - 1; i >= 0; i--)
            {
                if (this[i].CanBeTarget && (exclude == null || !exclude.Contains(this[i])))
                {
                    if (range <= 0 || range > 0 && SimpleMath.SqrDistXZ(this[i].Position, pos) < sqrRange)
                    {
                        results.Add(this[i]);
                    }
                }
            }
            if (results.Count > 0)
            {
                return results[Random.Range(0, results.Count)];
            }
            return null;
        }
#if !DACODER_RELEASE || UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void KillAll()
        {
            var l = new List<Actor.Actor>();
            foreach (var actor in List)
            {
                if (actor is Actor.Actor a)
                    l.Add(a);
                    
            }

            foreach (var actor in l)
                actor.InstantDead(new CombinedDamageForceData());
        }

        [Sirenix.OdinInspector.Button]
        public void DespawnAll()
        {
            var l = new List<Actor.Actor>();
            foreach (var actor in List)
            {
                if (actor is Actor.Actor a)
                    l.Add(a);
                    
            }

            foreach (var actor in l)
                actor.Despawn(actor.Position);
        }
#endif
    }
}