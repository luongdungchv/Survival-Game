using Dacodelaac.Collections;
using UnityEngine;

namespace BoxHead2.Collection
{
    [CreateAssetMenu(menuName = "Collections/CircleObstacleCollection")]
    public class CircleObstacleCollection : BaseCollection<CircleObstacle>
    {
    }
    
    public class CircleObstacle
    {
        public Vector3 Center;
        public float Radius;
    }
}