using System.Collections;
using System.Collections.Generic;
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
                    if (packet.atk) Debug.Log("attack");
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
            default:
                {
                    var packet = new ObjectInteractionPacket(parsedCmd);
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
    public MovePlayerPacket()
    {
        this.command = PacketType.MovePlayer;
    }
    public override string GetString()
        => $"{(int)command} {id} {position.x.ToString("0.00")} {position.y.ToString("0.00")} {position.z.ToString("0.00")} {anim}";

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
    public bool sprint, jump, atk;
    public InputPacket()
    {
        this.command = PacketType.Input;
    }
    public override string GetString()
        => $"{(int)command} {id} {inputVector.x} {inputVector.y} {(sprint ? 1 : 0)} {(jump ? 1 : 0)} {camDir.x.ToString("0.00")} {camDir.y.ToString("0.00")} {(atk ? 1 : 0)}";

    public override byte[] GetBytes() => Encoding.ASCII.GetBytes(GetString());
    public void WriteData(string _id, Vector2 _inputVector, bool _sprint, bool _jump, Vector2 camDir, bool atk)
    {
        this.id = _id;
        this.inputVector = _inputVector;
        this.jump = _jump;
        this.sprint = _sprint;
        this.camDir = camDir;
        this.atk = atk;
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
        }
    }
}
public class SpawnObjectPacket : Packet
{
    public string playerId;
    public int objSpawnId;
    public Vector3 position, rotation;
    public SpawnObjectPacket()
    {
        this.command = PacketType.SpawnObject;
    }
    public override string GetString()
        => $"{(int)command} {playerId} {objSpawnId} {position.x.ToString("0.00")} {position.y.ToString("0.00")} {position.z.ToString("0.00")} {rotation.x.ToString("0.00")} {rotation.y.ToString("0.00")} {rotation.z.ToString("0.00")}";
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if ((PacketType)int.Parse(split[0]) == this.command)
        {
            this.playerId = split[1];
            this.objSpawnId = int.Parse(split[2]);
            this.position = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
            this.rotation = new Vector3(float.Parse(split[6]), float.Parse(split[7]), float.Parse(split[8]));
        }
    }
    public void WriteData(string _playerId, int _objSpawnId, Vector3 _position, Vector3 _rotation)
    {
        this.playerId = _playerId;
        this.objSpawnId = _objSpawnId;
        this.position = _position;
        this.rotation = _rotation;
    }
}
public class UpdateEquippingPacket : Packet
{
    public string playerId;
    public string itemName;
    public UpdateEquippingPacket() { this.command = PacketType.UpdateEquipping; }
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
public class ObjectInteractionPacket : Packet
{
    public string playerId, objId, action;
    public ObjectInteractionPacket(PacketType type)
    {
        this.command = type;
    }
    public override string GetString() => $"{(int)command} {playerId} {objId} {action}";

    public void WriteData(string playerId, string objId, string action)
    {
        this.playerId = playerId;
        this.objId = objId;
        this.action = action;
    }
    public void WriteData(string msg)
    {
        var split = msg.Split(' ');
        if (int.Parse(split[0]) == (int)this.command)
        {
            this.playerId = split[1];
            this.objId = split[2];
            this.action = split[3];
        }
    }
}
public enum PacketType
{
    MovePlayer,
    SpawnPlayer, StartGame, Input, SpawnObject, UpdateEquipping,

    ChestInteraction, TreeInteraction, OreInteraction

}
