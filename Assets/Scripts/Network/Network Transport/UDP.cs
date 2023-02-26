using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDP
{
    public UdpClient socket;
    private Client owner;
    private ClientHandle handler => owner.handler;
    public string hostName;
    public int port;
    public UDP(Client owner)
    {
        this.owner = owner;
    }
    public void Connect(string hostName, int port)
    {
        socket = new UdpClient(0);
        socket.Connect(hostName, port);
        this.hostName = hostName;
        this.port = port;
        socket.BeginReceive(UDPReceiveCallback, null);
    }
    public bool Send(string msg)
    {
        return this.Send(Encoding.ASCII.GetBytes(msg));
    }
    public bool Send(byte[] data)
    {
        try
        {
            socket.BeginSend(data, data.Length, null, null);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
    }
 

    private void UDPReceiveCallback(IAsyncResult result)
    {
        var remoteEP = new IPEndPoint(IPAddress.Any, 0);
        try
        {
            var data = socket.EndReceive(result, ref remoteEP);
            var msg = Encoding.ASCII.GetString(data);
            handler.HandleMessage(msg);
            socket.BeginReceive(UDPReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Disconnect();
        }
    }
    public IPEndPoint GetSocketEP()
    {
        return this.socket.Client.LocalEndPoint as IPEndPoint;
    }
    public void Disconnect()
    {
        Debug.Log("UDP disconnected");
        UIManager.ins?.ShowDisconnectPanel();
        socket?.Close();
        socket = null;
    }
}
