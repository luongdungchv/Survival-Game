using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClientHandle : MonoBehaviour
{
    private Dictionary<PacketType, UnityAction<Packet>> handlers;
    private Client client => Client.ins;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        handlers = new Dictionary<PacketType, UnityAction<Packet>>();
    }
    public void HandleMessage(string msg)
    {
        var packet = Packet.ResolvePacket(msg);
        Debug.Log($"{msg}\n{packet.command}");
        handlers[packet.command](packet);
    }
    public void AddHandler(PacketType _packetType, UnityAction<Packet> _callback)
    {
        handlers.Add(_packetType, _callback);
    }
}
