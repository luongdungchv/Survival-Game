using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New State", menuName = "State")]
public class State : ScriptableObject
{
    public UnityEvent OnEnter;
    public UnityEvent OnUpdate;
    public UnityEvent OnExit;
    public List<State> transitions;
    public bool lockState;
}
