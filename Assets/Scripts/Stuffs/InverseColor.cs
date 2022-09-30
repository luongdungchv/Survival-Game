using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseColor : MonoBehaviour
{
    [SerializeField] private Material postProcessedMat;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, postProcessedMat);
    }
}
