using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Threading.Tasks;

public class StateMachine : MonoBehaviour
{
    public static UnityEvent<string, string> OnStateChanged = new UnityEvent<string, string>();
    public static StateMachine ins;
    public List<State> stateList;
    public State currentState;
    private void Awake()
    {
        if (ins == null) ins = this;
    }
    void Start()
    {
        OnStateChanged.AddListener((oldName, newName) =>
        {
            var rb = GetComponent<Rigidbody>();
            if (newName.Contains("Swim"))
            {

                rb.useGravity = false;
            }
            else rb.useGravity = true;
        });
    }

    void Update()
    {
        if (currentState != null) currentState.OnUpdate.Invoke();
    }
    public bool ChangeState(string name)
    {
        if (currentState.lockState)
        {
            return false;
        }
        //Debug.Log($"{currentState.name} {name}");
        if (name == currentState.name)
        {
            return false;
        };
        foreach (var i in currentState.transitions)
        {
            if (i.name == name)
            {
                OnStateChanged.Invoke(currentState.name, name);
                currentState.OnExit.Invoke();
                currentState = i;
                currentState.OnEnter.Invoke();
                return true;
            }
        }
        Debug.Log($"No Transition Found || {currentState.name}, {name}");
        //OnStateChanged.Invoke(name, "");
        return false;
    }
    public bool ChangeState(State newState)
    {
        if (newState.name == "InAir") Debug.Log("jump");
        return ChangeState(newState.name);
    }
    public async Task<bool> ChangeState(State newState, int delay)
    {
        await Task.Delay(delay);
        return ChangeState(newState);
    }

}
