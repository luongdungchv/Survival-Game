using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Threading.Tasks;

public class StateMachine : MonoBehaviour
{
    public static UnityEvent<string, string> OnStateChanged = new UnityEvent<string, string>();
    public List<State> stateList;
    public State currentState;
    private void OnEnable()
    {

    }
    void Start()
    {
        OnStateChanged.AddListener((oldName, newName) =>
        {
            var rb = GetComponent<Rigidbody>();
            Debug.Log($"{oldName} | {newName}");
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
        //        Debug.Log(name);
        if (currentState.lockState)
        {
            //OnStateChanged.Invoke(name, "");
            return false;
        }
        if (name == currentState.name)
        {
            //OnStateChanged.Invoke(name, "");
            return false;
        };
        //Debug.Log(name);
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
