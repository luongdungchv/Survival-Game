using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager ins;
    private Dictionary<string, NetworkObject> networkObjectList;

    [SerializeField] private NetworkPlayer playerPrefab;
    [SerializeField] private ClientHandle handler;
    private Dictionary<string, NetworkPlayer> playerList;
    private Client client => Client.ins;
    private ObjectMapper objMapper => ObjectMapper.ins;
    private void Start()
    {
        playerList = new Dictionary<string, NetworkPlayer>();
        DontDestroyOnLoad(this.gameObject);
        handler.AddHandler(PacketType.MovePlayer, HandleMovePlayer);
        handler.AddHandler(PacketType.SpawnPlayer, HandleSpawnPlayer);
        handler.AddHandler(PacketType.StartGame, HandleStartGame);
        handler.AddHandler(PacketType.Input, HandleInput);
        handler.AddHandler(PacketType.SpawnObject, HandleSpawnObject);
    }
    private void Awake()
    {
        ins = this;
        networkObjectList = new Dictionary<string, NetworkObject>();
    }
    private void HandleMovePlayer(Packet _packet)
    {
        try
        {
            var movePacket = _packet as MovePlayerPacket;
            var player = playerList[movePacket.id];
            player.ReceivePlayerState(movePacket);
        }
        catch
        {
            Debug.Log(_packet.GetString());
        }
    }
    private void HandleSpawnPlayer(Packet _packet)
    {
        var spawnPacket = _packet as SpawnPlayerPacket;
        if (spawnPacket.id != client.clientId)
        {
            var player = Instantiate(playerPrefab, spawnPacket.position, Quaternion.identity);
            player.id = spawnPacket.id;
            playerList.Add(player.id, player);
            if (client.isHost)
            {
                player.GetComponent<Rigidbody>().useGravity = true;
                //client.SendTCPMessage(spawnPacket.GetString());
                client.SendTCPPacket(spawnPacket);
            }
        }
    }
    private void HandleStartGame(Packet _packet)
    {
        var startPacket = _packet as StartGamePacket;
        client.clientId = startPacket.clientId;
        client.mapSeed = client.mapSeed;
        client.SetUDPRemoteHost(startPacket.udpRemoteHost);
        client.SendUDPMessage("con");
        client.StartCoroutine(LoadSceneDelay(2));
    }
    private void HandleInput(Packet _packet)
    {
        var inputPacket = _packet as InputPacket;
        var playerId = inputPacket.id;
        playerList[playerId].GetComponent<InputReceiver>().HandleInput(inputPacket);
    }
    public bool AddPlayer(string id, NetworkPlayer player)
    {
        if (!playerList.ContainsKey(id))
        {
            playerList.Add(id, player);
            return true;
        }
        else return false;
    }
    public void SpawnRequest(string playerId, NetworkPrefab prefab, Vector3 position, Vector3 rotation)
    {
        var prefabId = objMapper.GetPrefabIndex(prefab);
        Debug.Log($"Prefab id is: {prefabId}");
        if (prefabId != -1)
        {
            SpawnObjectPacket packet = new SpawnObjectPacket();
            packet.WriteData(playerId, prefabId, position, rotation);
            client.SendTCPPacket(packet);
        }
    }
    public void HandleSpawnObject(Packet _packet)
    {
        var spawnInfo = _packet as SpawnObjectPacket;
        var obj = Instantiate(objMapper.GetPrefab(spawnInfo.objSpawnId), spawnInfo.position, Quaternion.Euler(spawnInfo.rotation));
        if (client.isHost)
        {
            client.SendTCPPacket(spawnInfo);
        }
    }
    public void AddNetworkObject(string id, NetworkObject obj)
    {
        networkObjectList.Add(id, obj);
    }
    public NetworkObject GetNetworkObjecT(string id)
    {
        return networkObjectList[id];
    }
    public void RemoveNetworkObject(string id)
    {
        networkObjectList.Remove(id);
    }

    IEnumerator LoadSceneDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("Test_PlayerStats");
    }
}
