using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(MapGenerator)), CanEditMultipleObjects]
public class NoiseEditor : Editor
{


    public async override void OnInspectorGUI()
    {
        var obj = target as MapGenerator;
        if (DrawDefaultInspector())
        {
            try { await obj.MeshUpdate(); }
            catch
            {

            }
            //await obj.MeshUpdate();
        }
        //obj.UpdateMesh();
    }
}
