using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Security;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New State", menuName = "State")]
public class State : ScriptableObject
{
    public UnityEvent OnEnter;
    public UnityEvent OnUpdate;
    public UnityEvent OnExit;
    public UnityEvent OnFixedUpdate;
    public List<State> transitions;
    private Dictionary<string, bool> lockTransitions;
    public bool lockState;

    private void OnEnable()
    {
        lockTransitions = new Dictionary<string, bool>();
        if (transitions == null) transitions = new List<State>();
        foreach (var i in transitions)
        {
            lockTransitions.Add(i.name, false);
        }
    }

    public bool SetLock(string stateName, bool lockState)
    {
        if (lockTransitions.ContainsKey(stateName))
        {
            lockTransitions[stateName] = lockState;
            return true;
        }
        return false;
    }
    public bool SetLock(State state, bool lockState)
    {
        return SetLock(state.name, lockState);
    }
    public bool SetLock(State[] states, bool lockState)
    {
        bool res = false;
        foreach (var i in states)
        {
            if (lockTransitions.ContainsKey(i.name))
            {
                lockTransitions[i.name] = true;
                res = true;
            }
        }
        return res;
    }
    public bool CheckLock(string stateName)
    {
        if (!lockTransitions.ContainsKey(stateName)) return true;
        return lockTransitions[stateName];
    }
    public void LockAllTransitions()
    {
        foreach (var i in transitions)
        {
            lockTransitions[i.name] = true;
        }
    }
    public void UnlockAllTransitions()
    {
        foreach (var i in transitions)
        {
            lockTransitions[i.name] = false;
        }
    }
}
