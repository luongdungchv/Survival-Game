using System.Collections.Generic;
using System.Net.Cache;
using System.Security.AccessControl;
using UnityEngine;

public class PredropSpawner : MonoBehaviour
{
    [SerializeField] PredropType[] types;
    [SerializeField] private int size;

    private void Start()
    {
        SpawnPredrop();
    }

    private void SpawnPredrop()
    {
        foreach (var i in types)
        {

        }
    }
    private void RandomizeMap(int count)
    {
        int i = 0;
        var randObj = new CustomRandom(MapGenerator.ins.seed);
        int x = randObj.Next(0, size);
    }
}
[System.Serializable]
public class PredropType
{
    public string name;
    public GameObject prefab;
    public int count;
}