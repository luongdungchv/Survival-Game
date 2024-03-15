using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ShortMenu : Editor
{
[MenuItem("Open Scenes/AN_Demo")]//0
    public static void Option1(){
        string scenePath = 
"Assets/AZURE Nature/Demo/AN_Demo.unity";//s0
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
[MenuItem("Open Scenes/MainMenuRelease")]//1
    public static void Option2(){
        string scenePath = 
"Assets/Scenes/MainMenuRelease.unity";//s1
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
[MenuItem("Open Scenes/Test_PlayerStats")]//2
    public static void Option3(){
        string scenePath = 
"Assets/Scenes/Test Scenes/Test_PlayerStats.unity";//s2
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
[MenuItem("Open Scenes/Test1")]//3
    public static void Option4(){
        string scenePath = 
        "Assets/Scenes/Test1.unity";//s3
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
    //menu item placeholder4
    public static void Option5(){
        string scenePath = 
        "scene name placeholder4";
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
    //menu item placeholder5
    public static void Option6(){
        string scenePath = 
        "scene name placeholder5";
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
}
