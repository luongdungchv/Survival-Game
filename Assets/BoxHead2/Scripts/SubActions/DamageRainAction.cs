using System.Collections;
using System.Collections.Generic;
using BoxHead2.Actor;
using BoxHead2.Combat;
using UnityEngine;

namespace BoxHead2.SubActions
{
    [CreateAssetMenu(menuName = "Actions/DamageRainAction")]
    public class DamageRainAction : SubAction, ISubActionDataProvider<ExplodeActionData>
    {
        [SerializeField] int countDrop;
        [SerializeField] float interval;
        [SerializeField] ExplodeAction explodeAction;
        [SerializeField] float getEnemyRange = 6f;

        Vector3 _pos;
        DamageSourceData _damageSource;
        IActor _actor;
        HashSet<IDamageTaker> _except = new HashSet<IDamageTaker>();

        public override void Trigger(object target, IActor actor)
        {
            _actor = actor;
            _damageSource = Get<DamageRainActionData>(target).DamageSource;
            _except = new HashSet<IDamageTaker>();
            Stop();
            StartPerformRoutine();
        }

        public override void Detach(object target)
        {
            Stop();
        }

        protected override IEnumerator IEPerform()
        {
            for (var i = 0; i < countDrop; i++)
            {
                if (_damageSource.DamageSource == null || !_damageSource.DamageSource.Alive || _damageSource.DamageSource is IActor {Destroyed: true}) break;
                
                var enemy = _damageSource.DamageSource.GetRandomEnemy(getEnemyRange, except:_except);
                if (enemy is { Destroyed: false })
                {
                    _pos = enemy.Position;
                    var action = explodeAction.CreateCopy<SubAction>();
                    action.Trigger(this, _actor);
                    _except.Add(enemy);
                }
                else
                {
                    var otherEnemy = _damageSource.DamageSource.GetRandomEnemy(getEnemyRange);
                    if (otherEnemy is { Destroyed: false })
                    {
                        _pos = otherEnemy.Position;
                        var action = explodeAction.CreateCopy<SubAction>();
                        action.Trigger(this, _actor);
                    }
                }
                yield return new WaitForSeconds(interval);
            }

            Stop();
        }

        void Stop()
        {
            StopPerformRoutine();
        }

        ExplodeActionData ISubActionDataProvider<ExplodeActionData>.Get()
        {
            return new ExplodeActionData(_pos, _pos, 0, _damageSource, new DamageConfig(), false);
        }
    }

    public class DamageRainActionData
    {
        public DamageSourceData DamageSource;

        public DamageRainActionData(DamageSourceData damageSource)
        {
            DamageSource = damageSource;
        }
    }
}