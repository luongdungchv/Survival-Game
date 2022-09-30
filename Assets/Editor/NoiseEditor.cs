using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(MapGenerator)), CanEditMultipleObjects]
public class NoiseEditor : Editor
{
    // public override VisualElement CreateInspectorGUI()
    // {
    //     var baseGUI = new VisualElement();
    //     var obj = target as NoiseTexture;

    //     PropertyField width = new PropertyField();
    //     PropertyField height = new PropertyField();
    //     PropertyField scale = new PropertyField();

    //     PropertyField octaves = new PropertyField();
    //     PropertyField lacunarity = new PropertyField();
    //     PropertyField persistence = new PropertyField();

    //     PropertyField offset = new PropertyField();

    //     PropertyField terrainTypes = new PropertyField();
    //     PropertyField heightCurve = new PropertyField();

    //     var serializedObj = new SerializedObject(target as NoiseTexture);
    //     //

    //     height.bindingPath = "height";
    //     scale.bindingPath = "scale";
    //     width.bindingPath = "width";

    //     octaves.bindingPath = "octaves";
    //     lacunarity.bindingPath = "lacunarity";
    //     persistence.bindingPath = "persistence";

    //     offset.bindingPath = "offset";

    //     terrainTypes.bindingPath = "terrainTypes";
    //     heightCurve.bindingPath = "heightCurve";

    //     obj.UpdateMesh();

    //     Button updateBtn = new Button();
    //     updateBtn.text = "Update";
    //     updateBtn.clicked += () =>
    //     {
    //         Debug.Log(width.binding);
    //         Debug.Log(serializedObj.FindProperty("width").intValue);
    //         obj.UpdateTexture();
    //     };

    //     Debug.Log(baseGUI);

    //     baseGUI.Add(width);
    //     baseGUI.Add(height);
    //     baseGUI.Add(scale);
    //     baseGUI.Add(octaves);
    //     baseGUI.Add(lacunarity);
    //     baseGUI.Add(persistence);
    //     baseGUI.Add(offset);
    //     baseGUI.Add(terrainTypes);
    //     baseGUI.Add(heightCurve);
    //     baseGUI.Add(updateBtn);


    //     // baseGUI.Add(updateBtn);
    //     return null;
    // }

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
