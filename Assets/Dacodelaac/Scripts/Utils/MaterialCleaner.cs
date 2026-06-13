#if UNITY_EDITOR
using System.Linq;
using Dacodelaac.DebugUtils;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.Utils
{
    public static class MaterialCleaner
    {
        [MenuItem("Tools/Clean Material")]
        public static void Clean()
        {
            var materials = AssetUtils.FindAssetAtFolder<Material>(new[] {"Assets"});
            var serializedObjects = materials.Select(m => new SerializedObject(m)).ToArray();
            for (var i = 0; i < materials.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Clean material", $"Cleaning...({i}/{materials.Length})", i * 1f / materials.Length);
                serializedObjects[i].Update();
                ProcessProperties("m_SavedProperties.m_TexEnvs", materials[i], serializedObjects[i]);
                ProcessProperties("m_SavedProperties.m_Ints", materials[i], serializedObjects[i]);
                ProcessProperties("m_SavedProperties.m_Floats", materials[i], serializedObjects[i]);
                ProcessProperties("m_SavedProperties.m_Colors", materials[i], serializedObjects[i]);
            }
            Dacoder.Log("Done!");
            EditorUtility.ClearProgressBar();
        }

        static void ProcessProperties(string path, Material material, SerializedObject serializedObject)
        {
            var properties = serializedObject.FindProperty(path);
            if (properties != null && properties.isArray)
            {
                for (var i = 0; i < properties.arraySize; i++)
                {
                    var prop = properties.GetArrayElementAtIndex(i).FindPropertyRelative("first");
                    if (prop != null)
                    {
                        var propName = prop.stringValue;
                        if (material.HasProperty(propName)) continue;
                        properties.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        Dacoder.Log($"Remove {propName} from {AssetDatabase.GetAssetPath(material)}");
                    }
                }
            }
        }
    }
}
#endif