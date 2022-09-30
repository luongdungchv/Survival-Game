using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTester : MonoBehaviour
{
    public Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Debug.Log($"{mesh.vertices[i].ToString()}   {mesh.uv[i]}");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
