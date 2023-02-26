using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public class StateMachine : MonoBehaviour
{
    public static UnityEvent<string, string> OnStateChanged = new UnityEvent<string, string>();
    public State currentState;
    private void Awake()
    {
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
        if (currentState != null) currentState.OnUpdate.Invoke();
        
    }
    private void FixedUpdate()
    {
        if (currentState != null) {
            currentState.OnFixedUpdate.Invoke();
        }
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
        return false;
    }
    public bool ChangeState(State newState, bool force = false)
    {
        return ChangeState(newState.name, force);
    }
    public async Task<bool> ChangeState(State newState, int delay)
    {
        await Task.Delay(delay);
        return ChangeState(newState);
    }

}
