using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationMapper
{
    private static Dictionary<string, int> animMap = new Dictionary<string, int>(){
        {"Idle", 0},
        {"Move", 1},
        {"Sprint", 2},
        {"Dash", 3},
        {"InAir", 4},
        {"SwimNormal", 5},
        {"SwimIdle", 6},
    };
    public static int GetAnimationIndex(string animName)
    {
        if (!animMap.ContainsKey(animName)) return -1;
        return animMap[animName];
    }
    public static int GetAnimationIndex(State state)
    {
        return GetAnimationIndex(state.name);
    }
    public static string GetAnimationName(int index)
    {
        if (index < 0 || index >= animMap.Count) return null;
        return animMap.ElementAt(index).Key;
    }
}
