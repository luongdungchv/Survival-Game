using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager ins;
    private Dictionary<string, NetworkSceneObject> sceneObjects;

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
        handler.AddHandler(PacketType.UpdateEquipping, HandleChangeEquipment);
        handler.AddHandler(PacketType.ChestInteraction, HandleChestInteraction);
    }
    private void Awake()
    {
        ins = this;
        sceneObjects = new Dictionary<string, NetworkSceneObject>();
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
    public void HandleChangeEquipment(Packet _packet)
    {
        var updatePacket = _packet as UpdateEquippingPacket;
        if (playerList[updatePacket.playerId].TryGetComponent<NetworkEquipment>(out var netEquip))
        {
            netEquip.SetRightHandItem(Item.GetItem(updatePacket.itemName));
        }
        if (client.isHost) client.SendTCPPacket(updatePacket);
    }
    public void HandleChestInteraction(Packet _packet)
    {
        var chestPacket = _packet as ObjectInteractionPacket;
        var action = chestPacket.action;
        var obj = sceneObjects[chestPacket.objId];
        Debug.Log("Chest packet: " + chestPacket.ToString());
        if (action == "open")
        {
            obj.GetComponentInChildren<Chest>().Open();
        }
        if (client.isHost)
        {
            client.SendTCPPacket(chestPacket);
        }
    }
    public void AddNetworkSceneObject(string id, NetworkSceneObject obj)
    {
        sceneObjects.Add(id, obj);
    }
    public NetworkSceneObject GetNetworkSceneObject(string id)
    {
        return sceneObjects[id];
    }
    public bool RemoveSceneNetworkObject(string id)
    {
        if (!sceneObjects.ContainsKey(id)) return false;
        sceneObjects.Remove(id);
        return true;
    }

    IEnumerator LoadSceneDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("Test_PlayerStats");
    }
}
