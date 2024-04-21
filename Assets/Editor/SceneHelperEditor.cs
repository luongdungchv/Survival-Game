using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Codice.ThemeImages;
using System.Text.RegularExpressions;
using System.IO;

public class SceneHelperEditor : OdinEditorWindow
{
    [MenuItem("Window/Scene Helper")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneHelperEditor>();
        window.titleContent = new GUIContent("Scene Helper");
        window.Show();
        int i = 0;
        while (EditorPrefs.GetString("ScenePath" + i, null) != "")
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorPrefs.GetString("ScenePath" + i, ""));
            if (window.sceneAssetList == null) window.sceneAssetList = new List<SceneAsset>();
            if (asset != null) window.sceneAssetList.Add(asset);
            i++;
        }
    }

    //[OnValueChanged("SceneAssetListChange")]
    public List<SceneAsset> sceneAssetList;

    private void SceneAssetListChange()
    {
        var rootTextFile = File.ReadAllText("Assets/Editor/ShortMenuTemplate.txt");
        for (int i = 0; i < sceneAssetList.Count; i++)
        {
            var asset = sceneAssetList[i];
            var assetPath = AssetDatabase.GetAssetPath(asset);
            Debug.Log(assetPath);

            var menuItemMatch = "//menu item placeholder";

            var regexMenuItem = menuItemMatch + i;
            var regexScenePath = $"\"scene name placeholder{i}\";";
            //var fileContent = File.ReadAllText("Assets/Editor/ShortMenuTemplate.txt");

            Debug.Log(Regex.Match(rootTextFile, regexMenuItem).Length);

            if (Regex.Match(rootTextFile, regexMenuItem).Length == 0)
            {
                regexMenuItem = $".+?//{i}";
            }
            Debug.Log(Regex.Match(rootTextFile, regexScenePath).Length);
            if (Regex.Match(rootTextFile, regexScenePath).Length == 0)
            {
                regexScenePath = $".+?//s{i}";
            }

            string modified = Regex.Replace(rootTextFile, regexMenuItem, $"[MenuItem(\"Open Scenes/{asset.name}\")]//{i}");
            var final = Regex.Replace(modified, regexScenePath, $"\"{assetPath}\";//s{i}");
            rootTextFile = final;

            EditorPrefs.SetString("ScenePath" + i, assetPath);
        }
        File.WriteAllText("Assets/Editor/ShortMenu.cs", rootTextFile);
    }
    [Button]
    public void Save()
    {
        SceneAssetListChange();
        AssetDatabase.Refresh();
    }
    [Button]
    private void Test(){
        Debug.Log(Selection.activeObject.name);
        
    }
}
