using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    private void Start()
    {
        playerList = new Dictionary<string, NetworkPlayer>();
        DontDestroyOnLoad(this.gameObject);
        handler.AddHandler(PacketType.MovePlayer, HandleMovePlayer);
        handler.AddHandler(PacketType.SpawnPlayer, HandleSpawnPlayer);
        handler.AddHandler(PacketType.StartGame, HandleStartGame);
        handler.AddHandler(PacketType.Input, HandleInput);
        // Client.ins.OnTCPMessageReceive.AddListener((msg) =>
        // {
        //     var cmd = msg.Substring(0, 5);

        //     if (cmd == "spawn")
        //     {
        //         var split = msg.Split(' ');
        //         Debug.Log(cmd);
        //         var objName = split[1];
        //         var objId = split[2];
        //         if (objName == "player" && objId != client.clientId)
        //         {
        //             var pos = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
        //             var obj = Instantiate(playerPrefab, pos, Quaternion.identity);
        //             obj.id = objId;
        //             if (client.isHost)
        //             {
        //                 obj.GetComponent<Rigidbody>().useGravity = true;
        //                 client.SendTCPMessage(msg);
        //             }
        //         }
        //     }
        //     if (cmd == "udp")
        //     {
        //         Debug.Log(msg);
        //     }
        //});
    }
    private void Awake()
    {
        ins = this;
        networkObjectList = new Dictionary<string, NetworkObject>();
    }
    private void HandleMovePlayer(Packet _packet)
    {
        var movePacket = _packet as MovePlayerPacket;
        var player = playerList[movePacket.id];
        player.ReceivePosition(movePacket.position);
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
                client.SendTCPMessage(spawnPacket.GetString());
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
        Debug.Log(inputPacket.inputVector);
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
