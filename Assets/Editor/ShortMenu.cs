using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ShortMenu : Editor
{
//menu item placeholder0
    public static void Option1(){
        string scenePath = 
        "scene name placeholder0";
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
//menu item placeholder1
    public static void Option2(){
        string scenePath = 
        "scene name placeholder1";
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
//menu item placeholder2
    public static void Option3(){
        string scenePath = 
        "scene name placeholder2";
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(scenePath);
    }
//menu item placeholder3
    public static void Option4(){
        string scenePath = 
        "scene name placeholder3";
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
