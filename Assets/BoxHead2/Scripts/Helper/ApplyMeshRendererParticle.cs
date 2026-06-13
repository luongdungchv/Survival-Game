using UnityEngine;

namespace BoxHead2.Helper
{
    public class ApplyMeshRendererParticle : MonoBehaviour
    {
        [SerializeField] ParticleSystem particleSystem;

        public void ApplyMesh(Renderer mesh)
        {
            var shapeModule = particleSystem.shape;
            if (shapeModule.shapeType == ParticleSystemShapeType.SkinnedMeshRenderer && mesh is SkinnedMeshRenderer skinMesh)
            {
                shapeModule.skinnedMeshRenderer = skinMesh;
            }

            if (shapeModule.shapeType == ParticleSystemShapeType.MeshRenderer && mesh is MeshRenderer meshRenderer)
            {
                shapeModule.meshRenderer = meshRenderer;
            }
        }
    }
}
