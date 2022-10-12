using UnityEngine;

[CreateAssetMenu(menuName = "Item Data", fileName = "New Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject spawnAfterDrop, spawnWhenHolding;
    public Texture2D icon;


}