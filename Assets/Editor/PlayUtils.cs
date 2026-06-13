using UnityEditor;


public class PlayUtils : Editor
{
    [MenuItem("Games/Play Test Level")] //0
    public static void Option1()
    {
        Client.ins?.PlayTestLevel();
    }
}