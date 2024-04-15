using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Microsoft.SqlServer.Server;

public class RecurringAssetsViewer : OdinEditorWindow
{
    //[OnValueChanged("ListValueChangeCallback")]
    public List<Object> assetList;
    private RecurringAssetsHolderSO data;

    [MenuItem("Window/Recurring Assets")]
    public static void ShowWindow(){
        var window = GetWindow<RecurringAssetsViewer>();
        window.titleContent = new GUIContent("Recurring Assets");
        window.Show();
    }


    private void OnFocus() {
        data = AssetDatabase.LoadAssetAtPath<RecurringAssetsHolderSO>("Assets/Editor/SO/Recur.asset");
        if(data == null){
            data = ScriptableObject.CreateInstance<RecurringAssetsHolderSO>();
            AssetDatabase.CreateAsset(data, "Assets/Editor/SO/Recur.asset");
            AssetDatabase.SaveAssets();
        }

        assetList = data.assetList;
    }
    private void ListValueChangeCallback(){
        data.assetList = this.assetList;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
