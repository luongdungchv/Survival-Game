using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class Packet
{
    public static Packet ResolvePacket(string msg)
    {
        var cmd = msg.Substring(0, msg.IndexOf(" "));
        var parsedCmd = (PacketType)int.Parse(cmd);
        switch (parsedCmd)
        {
            case PacketType.MovePlayer:
                {
                    var packet = new MovePlayerPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.SpawnPlayer:
                {
                    var packet = new SpawnPlayerPacket();
                    packet.WriteData(msg);
                    Debug.Log("Spawn player: " + msg);
                    return packet;
                }
            case PacketType.StartGame:
                {
                    var packet = new StartGamePacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.Input:
                {
                    var packet = new InputPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.SpawnObject:
                {
                    var packet = new SpawnObjectPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.UpdateEquipping:
                {
                    var packet = new UpdateEquippingPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.FurnaceServerUpdate:
                {
                    var packet = new FurnaceUpdatePacket();
                    //Debug.Log(msg);
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.FurnaceClientMsg:
                {
                    var packet = new FurnaceClientMsgPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.ItemDrop:
                {
                    var packet = new ItemDropPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.RoomInteraction:
                {
                    var packet = new RoomPacket();
                    packet.WriteData(msg);
                    return packet;
                }
            case PacketType.UpdateEnemy:
            {
                var packet = new RawActionPacket(PacketType.UpdateEnemy);
                packet.WriteData(msg);
                return packet;
            }
            default:
                {
                    //                    Debug.Log("msg: " + msg);
                    var packet = new RawActionPacket(parsedCmd);
                    packet.WriteData(msg);
                    return packet;
                }
        }


    }
    public virtual string GetString()
    {
        return null;
    }
    public virtual byte[] GetBytes()
    {
        return null;
    }
    public PacketType command;
}
public class MovePlayerPacket : Packet
{
    public Vector3 position;
    public string id;
    public int anim;
    public int tick;
    public MovePlayerPacket()
    {
        this.command = PacketType.MovePlayer;
    }
    public override string GetString()
        => $"{(int)command} {id} {position.x.ToString("0.00")} {position.y.ToString("0.00")} {position.z.ToString("0.00")} {anim} {tick}";

    public override byte[] GetBytes() => Encoding.ASCII.GetBytes(this.GetString());
    public void WriteData(string _id, Vector3 _position, int _anim)
    {
        this.id = _id;
        this.position = _position;
        this.anim = _anim;
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if ((PacketType)int.Parse(split[0]) == PacketType.MovePlayer)
        {
            this.id = split[1];
            this.position = new Vector3(float.Parse(split[2]), float.Parse(split[3]), float.Parse(split[4]));
            this.anim = int.Parse(split[5]);
            this.tick = int.Parse(split[6]);
        }
    }
}
public class SpawnPlayerPacket : Packet
{
    public Vector3 position;
    public string id;
    public SpawnPlayerPacket()
    {
        this.command = PacketType.SpawnPlayer;
    }
    public override string GetString()
        => $"{(int)command} {id} {position.x.ToString()} {position.y.ToString()} {position.z.ToString()}";

    public override byte[] GetBytes() => Encoding.ASCII.GetBytes(this.GetString());
    public void WriteData(string _id, Vector3 _position)
    {
        this.id = _id;
        this.position = _position;
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if ((PacketType)int.Parse(split[0]) == PacketType.SpawnPlayer)
        {
            this.id = split[1];
            this.position = new Vector3(float.Parse(split[2]), float.Parse(split[3]), float.Parse(split[4]));
        }
    }
}
public class StartGamePacket : Packet
{
    public string clientId;
    public int mapSeed, udpRemoteHost;
    public StartGamePacket()
    {
        this.command = PacketType.StartGame;
    }
    public override string GetString()
        => $"{(int)command} {udpRemoteHost} {clientId} {mapSeed}";

    public override byte[] GetBytes() => Encoding.ASCII.GetBytes(GetString());

    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if ((PacketType)int.Parse(split[0]) == PacketType.StartGame)
        {
            this.udpRemoteHost = int.Parse(split[1]);
            this.clientId = split[2];
            this.mapSeed = int.Parse(split[3]);
        }
    }
}
public class InputPacket : Packet
{
    public string id;
    public Vector2 inputVector, camDir;
    public bool sprint, jump, atk, isConsumingItem;
    public int tick;
    public InputPacket()
    {
        this.command = PacketType.Input;
    }
    public override string GetString()
        => $"{(int)command} {id} {inputVector.x} {inputVector.y} {(sprint ? 1 : 0)} {(jump ? 1 : 0)} {camDir.x.ToString()} {camDir.y.ToString()} {(atk ? 1 : 0)} {(isConsumingItem ? 1 : 0)} {tick}";

    public override byte[] GetBytes() => Encoding.ASCII.GetBytes(GetString());
    public void WriteData(string _id, Vector2 _inputVector, bool _sprint, bool _jump, Vector2 camDir, bool atk, bool isConsumingItem)
    {
        this.id = _id;
        this.inputVector = _inputVector;
        this.jump = _jump;
        this.sprint = _sprint;
        this.camDir = camDir;
        this.atk = atk;
        this.isConsumingItem = isConsumingItem;
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if ((PacketType)int.Parse(split[0]) == this.command)
        {
            this.id = split[1];
            this.inputVector = new Vector2(int.Parse(split[2]), int.Parse(split[3]));
            this.sprint = int.Parse(split[4]) != 0;
            this.jump = int.Parse(split[5]) != 0;
            this.camDir = new Vector2(float.Parse(split[6]), float.Parse(split[7]));
            this.atk = int.Parse(split[8]) != 0;
            this.isConsumingItem = int.Parse(split[9]) != 0;
            this.tick = int.Parse(split[10]);
        }
    }
}
public class SpawnObjectPacket : Packet
{
    public string playerId;
    public int objPrefabIndex;
    public string objId;
    public Vector3 position, rotation;
    public SpawnObjectPacket()
    {
        this.command = PacketType.SpawnObject;
    }
    public override string GetString()
        => $"{(int)command} {playerId} {objPrefabIndex} {position.x.ToString("0.00")} {position.y.ToString("0.00")} {position.z.ToString("0.00")} {rotation.x.ToString("0.00")} {rotation.y.ToString("0.00")} {rotation.z.ToString("0.00")} {objId}";
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if ((PacketType)int.Parse(split[0]) == this.command)
        {
            this.playerId = split[1];
            this.objPrefabIndex = int.Parse(split[2]);
            this.position = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
            this.rotation = new Vector3(float.Parse(split[6]), float.Parse(split[7]), float.Parse(split[8]));
            this.objId = split[9];
        }
    }
    public void WriteData(string _playerId, int objPrefabIndex, Vector3 _position, Vector3 _rotation, string objId)
    {
        this.playerId = _playerId;
        this.objPrefabIndex = objPrefabIndex;
        this.position = _position;
        this.rotation = _rotation;
        this.objId = objId;
    }
}
public class UpdateEquippingPacket : Packet
{
    public string playerId;
    public string itemName;
    public UpdateEquippingPacket()
    {
        this.command = PacketType.UpdateEquipping;
        itemName = "0";
    }
    public override string GetString() => $"{(int)command} {playerId} {itemName}";
    public void WriteData(string playerId, string itemName)
    {
        this.playerId = playerId;
        this.itemName = itemName;
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if (int.Parse(split[0]) == (int)this.command)
        {
            this.playerId = split[1];
            this.itemName = split[2];
        }
    }

}
public class RawActionPacket : Packet
{
    public string playerId, objId, action;
    public string[] actionParams;
    public RawActionPacket(PacketType type)
    {
        this.command = type;
    }
    public override string GetString() => $"{(int)command} {playerId} {objId} {action} {string.Join("|", actionParams == null ? new string[0] : actionParams)}";

    public void WriteData(string playerId, string objId, string action, string[] actionParams)
    {
        this.playerId = playerId;
        this.objId = objId;
        this.action = action;
        this.actionParams = actionParams;
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if (int.Parse(split[0]) == (int)this.command)
        {
            this.playerId = split[1];
            this.objId = split.Length > 2 ? split[2] : "";
            this.action = split.Length > 3 ? split[3] : "";
            this.actionParams = split.Length > 4 ? split[4].Split('|') : new string[0];
        }
    }
}
public class FurnaceUpdatePacket : Packet
{
    public string playerId;
    public string objId;
    public string inputItem, fuelItem, outputItem;
    public int inputCount, fuelCount, outputCount;
    public int cookedUnit;
    public int remainingUnit;
    public FurnaceUpdatePacket()
    {
        this.command = PacketType.FurnaceServerUpdate;
        this.inputItem = "";
        this.fuelItem = "";
        this.outputItem = "";
    }
    public override string GetString()
    {
        var res = $"{(int)command} {playerId}{objId} {(char)(inputCount + 33)}{(char)(fuelCount + 33)}{(char)(outputCount + 33)} {inputItem}|{fuelItem}|{outputItem} {(char)(cookedUnit + 33)}{(char)(remainingUnit + 33)}";
        return res;
    }

    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if (int.Parse(split[0]) == (int)this.command)
        {
            this.playerId = split[1].Substring(0, 1);
            this.objId = split[1].Substring(1);

            var itemCountSplit = split[2];
            this.inputCount = (int)itemCountSplit[0] - 33;
            this.fuelCount = (int)itemCountSplit[1] - 33;
            this.outputCount = (int)itemCountSplit[2] - 33;

            var itemSplit = split[3].Split('|');
            this.inputItem = itemSplit[0];
            this.fuelItem = itemSplit[1];
            this.outputItem = itemSplit[2];

            //var cookProgressSplit = split[4].Split('|');
            this.cookedUnit = (int)(split[4][0]) - 33;
            this.remainingUnit = (int)(split[4][1]) - 33;
        }
    }
}
public class FurnaceClientMsgPacket : Packet
{
    public string playerId;
    public string objId;
    public string action;
    public string[] actionParams;
    public FurnaceClientMsgPacket()
    {
        this.command = PacketType.FurnaceClientMsg;
    }
    public override string GetString()
    {
        return $"{(int)command} {playerId} {objId} {action} {string.Join("|", actionParams)}";
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if (int.Parse(split[0]) == (int)command)
        {
            this.playerId = split[1];
            this.objId = split[2];
            this.action = split.Length > 3 ? split[3] : "";
            this.actionParams = split.Length > 4 ? split[4].Split('|') : new string[0];
        }
    }
}
public class ItemDropPacket : Packet
{
    public ItemDropPacket()
    {
        this.command = PacketType.ItemDrop;
    }
    public int objSpawnId;
    public Vector3 spawnPos;
    public string itemBase;
    public int quantity;
    public string objId;
    public override string GetString()
        => $"{(int)this.command} {objSpawnId} {(int)spawnPos.x} {(int)spawnPos.y} {(int)spawnPos.z} {itemBase} {quantity} {objId}";
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if (int.Parse(split[0]) == (int)this.command)
        {
            this.objSpawnId = int.Parse(split[1]);
            this.spawnPos = new Vector3(int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]));
            this.itemBase = split[5];
            this.quantity = int.Parse(split[6]);
            this.objId = split.Length > 7 ? split[7] : "";
        }
    }
}
public class RoomPacket : Packet
{
    public string action;
    public string[] args;
    public RoomPacket()
    {
        this.command = PacketType.RoomInteraction;
    }
    public override string GetString()
    {
        return base.GetString();
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');

        this.action = split[1];
        args = new string[split.Length - 2];
        for (int i = 2; i < split.Length; i++)
        {
            args[i - 2] = split[i];
        }

    }
}
public class SpawnEnemyPacket: Packet{
    public Vector3 spawnPosition;
    public string enemyNetId;
    
    public SpawnEnemyPacket(){
        this.command = PacketType.SpawnEnemy;
    }
    
    public override string GetString()
    {
        return $"{(int)this.command} {enemyNetId} {spawnPosition.x.ToString("0.00")} {spawnPosition.y.ToString("0.00")} {spawnPosition.z.ToString("0.00")}";
    }
    public void WriteData(string msg){
        var split = msg.Split(' ');
        enemyNetId = split[1];
        spawnPosition = new Vector3(int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]));
    }
}
public class UpdateEnemyPacket: Packet{
    public Vector3 position;
    public string objId;
    public string animTrigger;
    
    public UpdateEnemyPacket(){
        this.command = PacketType.UpdateEnemy;
    }
    
    public override string GetString()
    {
        return $"{(int)this.command} {objId} {position.x.ToString("0.00")} {position.y.ToString("0.00")} {position.z.ToString("0.00")} {animTrigger}";
    }
    public void WriteData(string msg){
        var split = msg.Split(' ');
        objId = split[1];
        position = new Vector3(float.Parse(split[2]), float.Parse(split[3]), float.Parse(split[4]));
        animTrigger = split[5];
    }
}
public class PowerupInteraction: Packet{
    public PowerupInteraction(){
        this.command = PacketType.PowerupInteraction;
    }
    public override string GetString()
    {
        return base.GetString();
    }
}

public enum PacketType
{
    MovePlayer,
    SpawnPlayer, StartGame, Input, SpawnObject, UpdateEquipping,
    FurnaceServerUpdate, FurnaceClientMsg,
    ItemDrop, RoomInteraction,
    SpawnEnemy, UpdateEnemy,PowerupInteraction,
    ChestInteraction, ItemDropObjInteraction, OreInteraction, DestroyObject, PlayerDisconnect, PlayerInteraction,
    

}
