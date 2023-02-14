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
        //socket.AllowNatTraversal(true);
        Debug.Log(GetSocketEP());
        socket.Connect(hostName, port);
        this.hostName = hostName;
        this.port = port;
        socket.BeginReceive(UDPReceiveCallback, null);
        //UDPReceiveAsync();
    }
    public bool Send(string msg)
    {
        var data = Encoding.ASCII.GetBytes(msg);
        try
        {
            socket.BeginSend(data, data.Length, null, null);
            return true;
        }
        catch (Exception e)
        {
            //Disconnect();
            // Debug.Log($"Cannot send: {msg}");
            Debug.Log(e.ToString());
            return false;
        }
    }
    public bool Send(string msg, int port)
    {
        var data = Encoding.ASCII.GetBytes(msg);
        try
        {
            //socket.BeginSend(data, data.Length, null, null);
            socket.BeginSend(data, data.Length, hostName, port, null, null);
            return true;
        }
        catch (Exception e)
        {
            //Disconnect();
            // Debug.Log($"Cannot send: {msg}");
            // Debug.Log(e.ToString());
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
