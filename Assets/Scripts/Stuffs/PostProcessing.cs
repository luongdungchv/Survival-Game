using UnityEngine;

public class PostProcessing : MonoBehaviour
{
    [SerializeField] private Material postProcessMat;
    void Start()
    {
        var cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
    }

}
