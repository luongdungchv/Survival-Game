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
        if (Client.ins.isHost && GetComponent<NetworkPlayer>().isLocalPlayer)
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
    }

    void Update()
    {
        //GetComponent<Rigidbody>().AddForce(0, -46, 0);
        if (currentState != null) currentState.OnUpdate.Invoke();
    }
    public bool ChangeState(string stateName, bool force = false)
    {
        if (currentState.lockState && !force)
        {
            return false;
        }

        if (stateName == currentState.name || currentState.CheckLock(stateName))
        {
            return false;
        };
        foreach (var i in currentState.transitions)
        {
            if (i.name == stateName)
            {
                OnStateChanged.Invoke(currentState.name, stateName);
                currentState.OnExit.Invoke();
                currentState = i;
                currentState.OnEnter.Invoke();
                return true;
            }
        }
        Debug.Log($"No Transition Found || {currentState.name}, {stateName}");
        //OnStateChanged.Invoke(name, "");
        return false;
    }
    public bool ChangeState(State newState, bool force = false)
    {
        if (newState.name == "InAir") Debug.Log("jump");
        return ChangeState(newState.name, force);
    }
    public async Task<bool> ChangeState(State newState, int delay)
    {
        await Task.Delay(delay);
        return ChangeState(newState);
    }

}
