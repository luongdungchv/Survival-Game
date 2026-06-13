using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class TextureArrayCreator : ScriptableObject
{
    [SerializeField] string path;
    [SerializeField] Texture2D[] textures;

#if UNITY_EDITOR
    [Button]
    public void Create()
    {
        var array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, TextureFormat.ARGB32,
            false);
        for (var i = 0; i < textures.Length; i++)
        {
            array.SetPixels(textures[i].GetPixels(), i);
        }

        array.Apply();
        AssetDatabase.CreateAsset(array, path + ".asset");
        AssetDatabase.Refresh();
    }
#endif
}
