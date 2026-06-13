using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Utils
{
    public static class AssetUtils
    {
#if UNITY_EDITOR
        public static void ChangeAssetName(Object asset, string name)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
            asset.name = name;
            AssetDatabase.RenameAsset(assetPath, name);
            AssetDatabase.SaveAssets();
        }

        public static T[] FindAssetAtFolder<T>(params string[] paths) where T : Object
        {
            var list = new List<T>();
            if (EditorApplication.isUpdating) return list.ToArray();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", paths);
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset)
                {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }
        public static T[] FindAssetAtFolderWithQuery<T>(string query, params string[] paths) where T : Object
        {
            var list = new List<T>();
            if (EditorApplication.isUpdating) return list.ToArray();
            var guids = AssetDatabase.FindAssets($"{query} t:{typeof(T).Name}", paths);
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset)
                {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }
        public static Object[] FindAssetAtFolder(System.Type type, params string[] paths)
        {
            var list = new List<Object>();
            if (EditorApplication.isUpdating) return list.ToArray();
            
            var guids = AssetDatabase.FindAssets($"t:{type.Name}", paths);
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type);
                if (asset)
                {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }
#endif
    }
}