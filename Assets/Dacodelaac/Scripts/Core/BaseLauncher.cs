using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dacodelaac.Core
{
    public class BaseLauncher : BaseMono
    {
        [SerializeField] GameObject[] prefabs;

        public override void Initialize()
        {
            base.Initialize();
            
            pools.Initialize();

            var list = new List<GameObject>();
            
            foreach (var prefab in prefabs)
            {
                list.Add(Instantiate(prefab));
            }
            
            foreach (var go in list)
            {
                pools.Initialize(go);
            }
        }
    }
}