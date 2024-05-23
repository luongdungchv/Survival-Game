using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMeshRenderer : MonoBehaviour
{
    [SerializeField] private Mesh mainMesh;
    [SerializeField] private List<Mesh> lodMesh;
    [SerializeField] private Material drawMaterial;
}
