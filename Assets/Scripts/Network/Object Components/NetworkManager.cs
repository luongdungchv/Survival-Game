﻿using System;
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
        handler.AddHandler(PacketType.FurnaceServerUpdate, HandleFurnaceServerUpdate);
        handler.AddHandler(PacketType.FurnaceClientMsg, HandleFurnaceClientMsg);
        handler.AddHandler(PacketType.ItemDropObjInteraction, HandlerItemDropObjInteraction);
        handler.AddHandler(PacketType.DestroyObject, HandleDestroyObject);
        handler.AddHandler(PacketType.ItemDrop, HandleDropItem);
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
        //client.SendUDPMessage("con");
        client.SendUDPConnectionInfo(() =>
        {
            StartCoroutine(LoadSceneDelay(0.5f));
        });
        //
    }
    private void HandleInput(Packet _packet)
    {
        var inputPacket = _packet as InputPacket;
        var playerId = inputPacket.id;
        if (playerList.ContainsKey(playerId))
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
    public void DropItemServer(NetworkPrefab prefab, Vector3 position, string itemBase, int quantity)
    {

    }
    public void HandleDropItem(Packet _packet)
    {
        var dropPacket = _packet as ItemDropPacket;
        if (dropPacket.objSpawnId != -1)
        {
            var prefab = objMapper.GetPrefab(dropPacket.objSpawnId);
            var drop = Instantiate(prefab.gameObject, dropPacket.spawnPos, Quaternion.identity).GetComponentInChildren<ItemDrop>();
            drop.SetQuantity(dropPacket.quantity);
            drop.SetBase(Item.GetItem(dropPacket.itemBase));

            var netSceneObj = drop.GetComponentInParent<NetworkSceneObject>();
            if (dropPacket.objId == "") dropPacket.objId = GameFunctions.ins.GenerateId();
            netSceneObj.id = dropPacket.objId;
            AddNetworkSceneObject(dropPacket.objId, netSceneObj);
            if (client.isHost) client.SendTCPPacket(dropPacket);
        }

    }
    public void HandleSpawnObject(Packet _packet)
    {
        var spawnInfo = _packet as SpawnObjectPacket;
        var obj = Instantiate(objMapper.GetPrefab(spawnInfo.objSpawnId), spawnInfo.position, Quaternion.Euler(spawnInfo.rotation));
        Debug.Log("spawn: " + spawnInfo.GetString());
        if (client.isHost)
        {
            client.SendTCPPacket(spawnInfo);
        }
    }
    public void HandleDestroyObject(Packet _packet)
    {
        var packet = _packet as ObjectInteractionPacket;
        var objId = packet.objId;
        Debug.Log("Destroy Msg: " + packet.GetString());
        if (sceneObjects.ContainsKey(objId))
        {
            Destroy(sceneObjects[objId].gameObject);
        }
    }
    public void HandleChangeEquipment(Packet _packet)
    {
        var updatePacket = _packet as UpdateEquippingPacket;
        Debug.Log("update: " + updatePacket.GetString());
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
    public void HandlerItemDropObjInteraction(Packet _packet)
    {
        // actionParams: tool, incomingDmg
        var packet = _packet as ObjectInteractionPacket;
        var action = packet.action;

        var playerId = packet.playerId;
        var tool = packet.actionParams[0];
        var incomingDmg = float.Parse(packet.actionParams[1]);


        var obj = sceneObjects[packet.objId];
        Debug.Log("Tree packet: " + packet.ToString());
        if (action == "take_dmg")
        {
            var objComponent = obj.GetComponent<ItemDropObject>();
            //objComponent.OnDamage(treePacket.actionParams[0],)
            var hitData = new PlayerHitData(incomingDmg, tool, playerList[playerId].GetComponent<PlayerStats>(), false);
            objComponent.OnDamage(hitData);

        }
        if (client.isHost)
        {
            client.SendTCPPacket(packet);
        }
    }
    public void HandleFurnaceClientMsg(Packet _packet)
    {
        var packet = _packet as FurnaceClientMsgPacket;
        var action = packet.action;
        var obj = sceneObjects[packet.objId].GetComponentInChildren<Transformer>();
        Debug.Log("action: " + action);
        switch (action)
        {
            case "set_input":
                {
                    var inputItem = Item.GetItem(packet.actionParams[0]) as ITransformable;
                    var quantity = int.Parse(packet.actionParams[1]);
                    obj.SetInput(inputItem, quantity);
                    break;
                }
            case "add_input":
                {
                    var inputItem = Item.GetItem(packet.actionParams[0]) as ITransformable;
                    var quantity = int.Parse(packet.actionParams[1]);
                    obj.AddInput(inputItem, quantity);
                    break;
                }
            case "set_fuel":
                {
                    var fuelItem = Item.GetItem(packet.actionParams[0]) as IFuel;
                    var quantity = int.Parse(packet.actionParams[1]);
                    obj.SetFuel(fuelItem, quantity);
                    break;
                }
            case "add_fuel":
                {
                    var fuelItem = Item.GetItem(packet.actionParams[0]) as IFuel;
                    var quantity = int.Parse(packet.actionParams[1]);
                    obj.AddFuel(fuelItem, quantity);
                    break;
                }
            case "retr_input":
                {
                    var quantity = int.Parse(packet.actionParams[0]);
                    obj.RetrieveInput(quantity);
                    break;
                }
            case "retr_fuel":
                {
                    var quantity = int.Parse(packet.actionParams[0]);
                    obj.RetrieveFuel(quantity);
                    break;
                }
            case "retr_output":
                {
                    var quantity = int.Parse(packet.actionParams[0]);
                    obj.RetrieveOutput(quantity);
                    break;
                }
        }

    }
    public void HandleFurnaceServerUpdate(Packet _packet)
    {
        var packet = _packet as FurnaceUpdatePacket;
        var obj = sceneObjects[packet.objId].GetComponentInChildren<TransformerClient>();
        var cookedUnit = packet.cookedUnit;
        var currentUnitCount = packet.remainingUnit;
        if (packet.inputItem == "")
        {
            obj.ReceiveInput(packet.inputCount);
        }
        else
        {
            obj.ReceiveInput(Item.GetItem(packet.inputItem) as ITransformable, packet.inputCount);
        }
        if (packet.fuelItem == "")
        {
            obj.ReceiveFuel(packet.fuelCount);
        }
        else
        {
            obj.ReceiveFuel(Item.GetItem(packet.fuelItem) as IFuel, packet.fuelCount);
        }
        if (packet.outputItem == "")
        {
            obj.ReceiveOutput(packet.outputCount);
        }
        else
        {
            obj.ReceiveOutput(Item.GetItem(packet.outputItem), packet.outputCount);
        }
        obj.ReceiveProgressInfo(packet.cookedUnit, packet.remainingUnit);
        UIManager.ins.RefreshFurnaceUI();
    }
    public void AddNetworkSceneObject(string id, NetworkSceneObject obj)
    {
        if (sceneObjects.ContainsKey(id))
            sceneObjects[id] = obj;
        else sceneObjects.Add(id, obj);
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
        Debug.Log("load");
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("Test_PlayerStats");
    }
}
