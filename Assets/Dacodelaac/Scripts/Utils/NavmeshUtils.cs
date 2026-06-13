using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Dacodelaac.Utils
{
    public static class NavmeshUtils
    {
        public static Vector3 GetRandomPositionOnNavmesh()
        {
            var triangulation = NavMesh.CalculateTriangulation();
            var verts = triangulation.vertices;
            if (verts == null || verts.Length == 0) return Vector3.zero;
            verts.Shuffle(true);
            var (p1, p2) = (verts[0], verts[1]);
            return Vector3.Lerp(p1, p2, CryptoRandom.value);
        }
    }
}