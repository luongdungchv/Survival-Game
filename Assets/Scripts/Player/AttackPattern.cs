using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPattern : MonoBehaviour
{
    public string type;
    public int attackCount;
    public float[] resetDelay;
    public float[] delayBetweenMoves;
    public float[] displaceForward;
    [SerializeField] private HitBox[] hitBoxes;
    public void DetectHit(int boxIndex)
    {
        //Debug.Log("hit call");
        hitBoxes[boxIndex].DetectHit();
        //var center = box.center
    }
}
