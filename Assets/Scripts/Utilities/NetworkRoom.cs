using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkRoom
{
    private UnityEvent<string> OnPlayerJoin;

    private UnityEvent<int> OnPlayerLeave;
    public UnityEvent<int, bool> OnPlayerReady;
    public static NetworkRoom ins;
    public string id;
    public int mapSeed;
    public int localPlayerId;
    public List<string> playerNames;
    public List<bool> readyStates;
    public int playerCount => playerNames.Count;
    public NetworkRoom()
    {
        ins = this;
        playerNames = new List<string>();
        readyStates = new List<bool>();
        OnPlayerJoin = new UnityEvent<string>();
        OnPlayerLeave = new UnityEvent<int>();
        OnPlayerReady = new UnityEvent<int, bool>();
    }
    public void StartGame()
    {
        foreach (var i in readyStates)
        {
            if (!i) return;
        }
        var loadSceneTask = SceneManager.LoadSceneAsync("Test_PlayerStats");
        loadSceneTask.completed += (a) =>
        {
            MapGenerator.ins.seed = mapSeed;
        };

    }
    public void AddPlayer(string playerId)
    {
        this.playerNames.Add(playerId);
        this.readyStates.Add(false);
        OnPlayerJoin.Invoke(playerId);
    }
    public void RemovePlayer(string playerId)
    {
        var index = playerId.IndexOf(playerId);
        this.playerNames.RemoveAt(index);
        this.readyStates.RemoveAt(index);
        OnPlayerLeave.Invoke(index);
    }
    public void RemovePlayer(int index)
    {
        this.playerNames.RemoveAt(index);
        this.readyStates.RemoveAt(index);
        OnPlayerLeave.Invoke(index);
    }
    public int GetPlayerId(string name)
    {
        return playerNames.IndexOf(name);
    }
    public void SetReadyState(int index, bool state)
    {
        readyStates[index] = state;
        OnPlayerReady.Invoke(index, state);
    }
    public void RegisterJoinEvent(UnityAction<string> callback)
    {
        OnPlayerJoin.AddListener(callback);
    }
    public void RegisterLeaveEvent(UnityAction<int> callback)
    {
        OnPlayerLeave.AddListener(callback);
    }
    public void RegisterReadyEvent(UnityAction<int, bool> callback)
    {
        OnPlayerReady.AddListener(callback);
    }
    public void UnRegisterJoinEvent(UnityAction<string> callback)
    {
        OnPlayerJoin.RemoveListener(callback);
    }
    public void UnRegisterLeaveEvent(UnityAction<int> callback)
    {
        OnPlayerLeave.RemoveListener(callback);
    }
    public void UnRegisterReadyEvent(UnityAction<int, bool> callback)
    {
        OnPlayerReady.RemoveListener(callback);
    }

}
