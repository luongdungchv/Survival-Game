using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attack Pattern", fileName = "New Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    public string type;
    public int attackCount;
    public float[] resetDelay;
    public float[] delayBetweenMoves;
    public float[] displaceForward;
}
