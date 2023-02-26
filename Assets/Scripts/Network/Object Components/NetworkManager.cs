using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Enemy.Base;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager ins;
    private Dictionary<string, NetworkSceneObject> sceneObjects;

    [SerializeField] private NetworkPlayer playerPrefab;
    [SerializeField] private ClientHandle handler;
    public GameObject test;
    private RoomUIManager roomUI;
    private Dictionary<string, NetworkPlayer> playerList;
    private Client client => Client.ins;
    private ObjectMapper objMapper => ObjectMapper.ins;
    public bool gameStarted;
    private void Awake()
    {
        if (ins == null) ins = this;
        else
        {
            Destroy(ins.gameObject);
            ins = this;
        }
        sceneObjects = new Dictionary<string, NetworkSceneObject>();
        gameStarted = false;
    }
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
        handler.AddHandler(PacketType.InventoryInteraction, HandleInventoryPacket);
        handler.AddHandler(PacketType.ItemDropObjInteraction, HandleSceneObjInteraction);
        handler.AddHandler(PacketType.DestroyObject, HandleDestroyObject);
        handler.AddHandler(PacketType.ItemDrop, HandleDropItem);
        handler.AddHandler(PacketType.PlayerDisconnect, HandlePlayerDisconnect);
        handler.AddHandler(PacketType.RoomInteraction, HandleRoomInteraction);
        handler.AddHandler(PacketType.UpdateEnemy, HandleEnemyState);
        handler.AddHandler(PacketType.PlayerInteraction, HandlePlayerInteraction);
        handler.AddHandler(PacketType.ShipInteraction, HandleShipInteraction);

    }
    private void HandleMovePlayer(Packet _packet)
    {

        try
        {
            var movePacket = _packet as MovePlayerPacket;
            var player = playerList[movePacket.id];
            if (player.GetComponent<PlayerStats>().isDead) return;
            player.HandlePlayerState(movePacket);
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
            player.GetComponent<PlayerSpawner>().SetMarkerColor(int.Parse(spawnPacket.id));
            playerList.Add(player.id, player);
            if (client.isHost)
            {
                client.SendTCPPacket(spawnPacket);
            }
        }
    }
    private void HandleStartGame(Packet _packet)
    {
        var startPacket = _packet as StartGamePacket;
        client.clientId = startPacket.clientId;
        client.mapSeed = startPacket.mapSeed;
        client.SetUDPRemoteHost(startPacket.udpRemoteHost);
        client.SendUDPConnectionInfo(() =>
        {
            StartCoroutine(LoadScene(1));
            gameStarted = true;
        });


    }
    private int tick = -1;
    private int lastClientTick = -1;
    private void HandleInput(Packet _packet)
    {
        var inputPacket = _packet as InputPacket;

        var lastTickDiff = lastClientTick - inputPacket.tick;
        if (tick == -1 || (lastTickDiff > 0 && lastTickDiff < 500)) tick = inputPacket.tick;
        else
        {
            tick += 1;
            if (tick > 1024) tick = 0;
            inputPacket.tick = tick;
        }
        var playerId = inputPacket.id;
        if (playerList.ContainsKey(playerId))
            playerList[playerId].GetComponent<InputReceiver>().AddPacket(inputPacket);
        lastClientTick = inputPacket.tick;
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
    public void SpawnRequest(string playerId, NetworkPrefab prefab, Vector3 position, Vector3 rotation, string objId)
    {
        var prefabId = objMapper.GetPrefabIndex(prefab);
        if (prefabId != -1)
        {
            SpawnObjectPacket packet = new SpawnObjectPacket();
            packet.WriteData(playerId, prefabId, position, rotation, objId);
            client.SendTCPPacket(packet);
        }
    }
    public void HandleRoomInteraction(Packet _packet)
    {
        var roomPacket = _packet as RoomPacket;
        var action = roomPacket.action;
        var args = roomPacket.args;
        if (action == "create" || action == "join")
        {
            NetworkRoom room = new NetworkRoom()
            {
                id = args[0],
                mapSeed = int.Parse(args[1])
            };
            for (int i = 2; i < args.Length; i++)
            {
                var playerName = args[i].Substring(0, args[i].Length - 1);
                var playerReadyState = int.Parse(args[i].Substring(args[i].Length - 1)) != 0 && i != 2;
                room.AddPlayer(args[i], playerReadyState);
            }
            room.localPlayerId = room.playerCount - 1;
            SceneManager.LoadScene("Room Scene");
        }
        else if (action == "room_add")
        {
            NetworkRoom.ins.AddPlayer(args[0]);
        }
        else if (action == "leave")
        {
            var playerIdToRemove = int.Parse(args[0]);
            NetworkRoom.ins.RemovePlayer(playerIdToRemove);
            if ((playerIdToRemove == NetworkRoom.ins.localPlayerId || playerIdToRemove == 0) && !gameStarted)
            {
                SceneManager.LoadScene(0);
            }
        }
        else if (action == "ready")
        {
            var playerId = int.Parse(args[0]);
            var state = int.Parse(args[1]) != 0;
            NetworkRoom.ins.SetReadyState(playerId, state);
        }
    }
    public void HandleDropItem(Packet _packet)
    {
        var dropPacket = _packet as ItemDropPacket;
        if (dropPacket.objSpawnId != -1)
        {
            var prefab = objMapper.GetPrefab(dropPacket.objSpawnId);
            var drop = Instantiate(prefab.gameObject, dropPacket.spawnPos, Quaternion.identity).GetComponentInChildren<ItemDrop>();
            drop.SetQuantity(dropPacket.quantity);

            var itemBase = Item.GetItem(dropPacket.itemBase);
            drop.SetBase(itemBase);
            drop.displayName = itemBase.dropDisplayName;
            drop.meshTex = itemBase.dropTexture;
            drop.outlineColor = itemBase.dropOutlineColor;

            var netSceneObj = drop.GetComponentInParent<NetworkSceneObject>();
            if (dropPacket.objId == "" || dropPacket.objId == "0") dropPacket.objId = GameFunctions.GenerateId();
            netSceneObj.id = dropPacket.objId;
            AddNetworkSceneObject(dropPacket.objId, netSceneObj);
            if (client.isHost) client.SendTCPPacket(dropPacket);
        }

    }
    public void HandleSpawnObject(Packet _packet)
    {
        var spawnInfo = _packet as SpawnObjectPacket;
        var obj = Instantiate(objMapper.GetPrefab(spawnInfo.objPrefabIndex), spawnInfo.position, Quaternion.Euler(spawnInfo.rotation));
        var netSceneObj = obj.GetComponent<NetworkSceneObject>();
        if (spawnInfo.objId != "0")
        {
            netSceneObj.id = spawnInfo.objId;
            sceneObjects.Add(netSceneObj.id, netSceneObj);
        }
        else
        {
            var objId = netSceneObj.GenerateId();
            spawnInfo.objId = objId;
        }
        if (client.isHost)
        {
            client.SendTCPPacket(spawnInfo);
        }
    }
    public void HandleDestroyObject(Packet _packet)
    {
        var packet = _packet as RawActionPacket;
        var objId = packet.objId;
        if (sceneObjects.ContainsKey(objId))
        {
            Destroy(sceneObjects[objId].gameObject);
        }
        if (client.isHost)
        {
            client.SendTCPPacket(packet);
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
        var chestPacket = _packet as RawActionPacket;
        var action = chestPacket.action;
        var obj = sceneObjects[chestPacket.objId];
        if (action == "open")
        {
            obj.GetComponentInChildren<Chest>().Open(playerList[chestPacket.playerId]);
        }
        if (client.isHost)
        {
            client.SendTCPPacket(chestPacket);
        }
    }
    public void HandleSceneObjInteraction(Packet _packet)
    {
        var packet = _packet as RawActionPacket;
        var action = packet.action;

        var playerId = packet.playerId;
        var tool = packet.actionParams[0];
        var incomingDmg = float.Parse(packet.actionParams[1]);

        var isCrit = int.Parse(packet.actionParams[2]) != 0;
        var isKnockback = int.Parse(packet.actionParams[3]) != 0;

        var obj = sceneObjects[packet.objId];
        if (action == "take_dmg")
        {
            var objComponent = obj.GetComponent<IDamagable>();
            var hitData = new PlayerHitData(incomingDmg, tool, playerList[playerId].GetComponent<PlayerStats>(), isCrit, isKnockback);
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
        var obj = sceneObjects[packet.objId].GetComponentInChildren<TransformerBase>();
        switch (action)
        {
            case "set_input":
                {
                    var item = Item.GetItem(packet.actionParams[0]);
                    var inputItem = item as ITransformable;
                    var quantity = int.Parse(packet.actionParams[1]);
                    if (!obj.SetInput(inputItem, quantity) && client.isHost)
                    {
                        var returnPacket = new RawActionPacket(PacketType.InventoryInteraction)
                        {
                            playerId = packet.playerId,
                            objId = "0",
                            action = "add_item",
                            actionParams = new string[] { item.itemName, quantity.ToString() },
                        };
                        client.SendTCPPacket(returnPacket);
                    }
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
            case "open":
                {
                    if (client.isHost)
                    {
                        if (!obj.isOpen)
                        {
                            client.SendTCPPacket(packet);
                            obj.isOpen = true;
                        }
                    }
                    else if (packet.playerId == NetworkPlayer.localPlayer.id)
                        obj.Open();

                    break;
                }
            case "close":
                {
                    if (client.isHost)
                    {
                        obj.isOpen = false;
                    }
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
    public void HandleInventoryPacket(Packet packet)
    {
        var inventoryPacket = packet as RawActionPacket;
        var playerId = inventoryPacket.playerId;
        var args = inventoryPacket.actionParams;
        var action = inventoryPacket.action;
        if (action == "add_item" && playerId == NetworkPlayer.localPlayer.id)
        {
            Inventory.ins.Add(args[0], int.Parse(args[1]));
        }
    }
    public void HandlePlayerDisconnect(Packet _packet)
    {
        var packet = _packet as RawActionPacket;
        if (playerList.TryGetValue(packet.playerId, out var playerToRemove))
        {
            Destroy(playerToRemove.gameObject);
            playerList.Remove(packet.playerId);
        }
    }
    public void HandlePlayerInteraction(Packet packet)
    {
        var playerPacket = packet as RawActionPacket;
        var action = playerPacket.action;
        var playerId = playerPacket.playerId;
        var args = playerPacket.actionParams;

        var player = playerList[playerId].GetComponent<PlayerStats>();
        if (player.isDead) return;

        if (action == "take_damage")
        {
            if (playerId == NetworkPlayer.localPlayer.id)
            {
                var dmg = int.Parse(args[0]);
                player.TakeDamage(dmg);
            }
            if (client.isHost) client.SendTCPPacket(playerPacket);
        }
        else if (action == "die")
        {
            player.Perish();
        }

    }
    public void HandleShipInteraction(Packet packet)
    {
        var shipPacket = packet as RawActionPacket;
        var playerId = shipPacket.playerId;

        var action = shipPacket.action;
        var args = shipPacket.actionParams;


        if (action == "set_item")
        {
            var itemName = args[0];
            var itemQuantity = int.Parse(args[1]);
            ShipRepair.ins.SetItem(Item.GetItem(itemName), itemQuantity);
            if (Client.ins.isHost)
            {
                client.SendTCPPacket(shipPacket);
            }
        }
        else if (action == "open")
        {
            if (Client.ins.isHost)
            {
                if (!ShipRepair.ins.isOpen)
                {
                    ShipRepair.ins.isOpen = true;
                    client.SendTCPPacket(packet);
                }
            }
            else
            {
                if (playerId == NetworkPlayer.localPlayer.id)
                {
                    UIManager.ins.ToggleShipRepairUI(true);
                }
            }
        }
        else if (action == "close")
        {
            if (Client.ins.isHost)
            {
                ShipRepair.ins.isOpen = false;
            }
        }
    }
    public void HandleEnemyState(Packet packet)
    {
        var updatePacket = packet as RawActionPacket;
        var action = updatePacket.action;
        var args = updatePacket.actionParams;
        if (sceneObjects.TryGetValue(updatePacket.objId, out var netObj))
        {
            var stats = netObj.GetComponent<EnemyStats>();
            var animator = netObj.GetComponent<Animator>();
            if (action == "set_target")
            {
                stats.target = playerList[args[0]].transform;
                SetTrigger(animator, "chase");
            }
            else if (action == "idle")
            {
                SetTrigger(animator, "idle");
            }
            else if (action == "patrol")
            {
                stats.targetPos = new Vector3(
                    float.Parse(args[0]),
                    float.Parse(args[1]),
                    float.Parse(args[2])
                );
                SetTrigger(animator, "patrol");
            }
            else if (action == "attack")
            {
                SetTrigger(animator, "atk");
            }
        }
        void SetTrigger(Animator animator, string trigger)
        {
            animator.ResetTrigger("atk");
            animator.ResetTrigger("idle");
            animator.ResetTrigger("chase");
            animator.ResetTrigger("patrol");
            animator.SetTrigger(trigger);
        }
    }
    public void AddNetworkSceneObject(string id, NetworkSceneObject obj)
    {
        if (sceneObjects.ContainsKey(id))
            sceneObjects[id] = obj;
        else sceneObjects.Add(id, obj);
    }
    public Dictionary<string, NetworkPlayer> GetAllPlayers() => this.playerList;
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
    public void SetRoomUIManager(RoomUIManager manager)
    {
        this.roomUI = manager;
    }
    IEnumerator LoadSceneDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("Test_PlayerStats");
        gameStarted = true;
    }
    private IEnumerator LoadScene(int buildIndex)
    {
        var asyncOperation = SceneManager.LoadSceneAsync(buildIndex);
        this.roomUI.OpenLoadingPanel();
        while (!asyncOperation.isDone)
        {
            this.roomUI.SetLoadingProgress(asyncOperation.progress);
            yield return null;
        }
    }
}

