using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;

public class Packet
{
    public static Packet ResolvePacket(string msg)
    {
        var cmd = msg.Substring(0, msg.IndexOf(" "));
        switch ((PacketType)int.Parse(cmd))
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
            default:
                return null;
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
    {
        return $"{(int)command} {id} {position.x.ToString()} {position.y.ToString()} {position.z.ToString()} {anim}";
    }
    public override byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(this.GetString());
    }
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
    {
        return $"{(int)command} {id} {position.x.ToString()} {position.y.ToString()} {position.z.ToString()}";
    }
    public override byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(this.GetString());
    }
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
    public int mapSeed;
    public int udpRemoteHost;
    public StartGamePacket()
    {
        this.command = PacketType.StartGame;
    }
    public override string GetString()
    {
        return $"{(int)command} {udpRemoteHost} {clientId} {mapSeed}";
    }
    public override byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(GetString());
    }
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
    public Vector2 inputVector;
    public bool sprint;
    public bool jump;
    public Vector2 camDir;
    public InputPacket()
    {
        this.command = PacketType.Input;
    }
    public override string GetString()
    {
        return $"{(int)command} {id} {inputVector.x} {inputVector.y} {(sprint ? 1 : 0)} {(jump ? 1 : 0)} {camDir.x} {camDir.y}";
    }
    public override byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(GetString());
    }
    public void WriteData(string _id, Vector2 _inputVector, bool _sprint, bool _jump, Vector2 camDir)
    {
        this.id = _id;
        this.inputVector = _inputVector;
        this.jump = _jump;
        this.sprint = _sprint;
        this.camDir = camDir;
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
        }
    }
}
public enum PacketType
{
    MovePlayer,
    SpawnPlayer, StartGame, Input
}
