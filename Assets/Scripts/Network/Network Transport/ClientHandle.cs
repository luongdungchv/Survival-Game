using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClientHandle : MonoBehaviour
{
    public static ClientHandle ins;
    private Dictionary<PacketType, UnityAction<Packet>> handlers;
    private Client client => Client.ins;
    private void Awake()
    {
        if (ins == null) ins = this;
        else
        {
            Destroy(ins.gameObject);
            ins = this;
        }
        DontDestroyOnLoad(this.gameObject);
        handlers = new Dictionary<PacketType, UnityAction<Packet>>();
    }
    public void HandleMessage(string msg)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            var packet = Packet.ResolvePacket(msg);
            handlers[packet.command](packet);
        });
    }
    public void AddHandler(PacketType _packetType, UnityAction<Packet> _callback)
    {
        handlers.Add(_packetType, _callback);
    }
}
