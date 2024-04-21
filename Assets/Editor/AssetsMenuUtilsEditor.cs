using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class AssetsMenuUtilsEditor : MonoBehaviour
{
    private static double renameDelay;
    //[MenuItem("Assets/Create/Scripts/C# Scriptable Object", false, 80)]
    public static void CreateScript()
    {
        EditorApplication.projectChanged -= OnHierarchyChanged;
        // Create a new script asset
        string scriptPath = AssetDatabase.GetAssetPath(Selection.objects[0]) + "/NewScriptableObjectScript.cs";
        var template = "Assets/Editor/Script Templates/ScriptableObjectTemplate.txt";
        var fileContent = File.ReadAllText(template);
        File.WriteAllText(scriptPath, "");

        // Refresh the AssetDatabase to detect the new file
        //AssetDatabase.Refresh();
        

        // Select the newly created file
        Object scriptAsset = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(Object));
        Selection.objects = new Object[] { scriptAsset };
        //EditorGUIUtility.PingObject(scriptAsset);
        
        // Trigger rename mode
        //AssetDatabase.Refresh();
        
        AssetDatabase.Refresh();
        renameDelay = EditorApplication.timeSinceStartup + 0.1;
        EditorApplication.update += ExecuteMenuItem;
    }
    private static void OnHierarchyChanged()
    {
        // Get the modified name after the rename operation
        EditorApplication.projectChanged -= OnHierarchyChanged;
        string modifiedScriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string modifiedScriptName = Path.GetFileNameWithoutExtension(modifiedScriptPath);
        

        //string modifiedScriptName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(scriptAsset));
        var fileContent = File.ReadAllText("Assets/Editor/Script Templates/ScriptableObjectTemplate.txt");
        var path = modifiedScriptPath.Substring(0, modifiedScriptPath.LastIndexOf("/") + 1) + modifiedScriptName + ".cs";
        Debug.Log(Regex.Match(fileContent, "#ScriptName#"));
        fileContent = Regex.Replace(fileContent, "#ScriptName#", modifiedScriptName);
        Debug.Log(fileContent); 
        //File.WriteAllText(path, fileContent); 
    } 

    private static void ExecuteMenuItem(){
        Debug.Log(EditorApplication.timeSinceStartup - renameDelay);
        if(EditorApplication.timeSinceStartup > renameDelay){
            EditorApplication.ExecuteMenuItem("Assets/Rename");
            EditorApplication.update -= ExecuteMenuItem;
            EditorApplication.projectChanged += OnHierarchyChanged;
            AssetDatabase.Refresh();
        }
    }
    private static void EditorUpdate(){
        Debug.Log(Event.current);
        if(
            (
                (Event.current.type == EventType.MouseDown && Event.current.button == 0) ||
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            ) &&
            EditorWindow.focusedWindow.GetType().ToString() == "UnityEditor.ProjectBrowser"

        ){
            OnHierarchyChanged();
        }
    }
}