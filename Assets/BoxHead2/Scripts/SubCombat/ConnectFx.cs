using Dacodelaac.Core;
using UnityEngine;

namespace BoxHead2.SubCombat
{
    public class ConnectFx : BaseMono
    {
        [SerializeField] Vector3 offset;
        [SerializeField] GameObject scale;
        [SerializeField] GameObject defaultAim;
        [SerializeField] Vector3 defaultScale;
        public void Setup(Vector3 distance, Quaternion aimedDirection)
        {
            scale.transform.localScale = distance;
            transform.rotation = aimedDirection * Quaternion.Euler(offset);
            if (defaultAim != null)
            {
                defaultAim.transform.localScale = new Vector3(defaultScale.x / distance.x, defaultScale.y / distance.y, defaultScale.z / distance.z);
            }
        }
    }
}