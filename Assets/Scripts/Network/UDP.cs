using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDP
{
    private UdpClient socket;
    private Client owner;
    private ClientHandle handler => owner.handler;
    public UDP(Client owner)
    {
        this.owner = owner;
    }
    public void Connect(string hostName, int port)
    {
        socket = new UdpClient(0);
        socket.Connect(hostName, port);
        //socket.BeginReceive(UDPReceiveCallback, null);
        UDPReceiveAsync();
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
            Debug.Log($"Cannot send: {msg}");
            Debug.Log(e.ToString());
            return false;
        }
    }

    private async void UDPReceiveAsync()
    {
        while (true)
        {
            var result = await socket.ReceiveAsync();
            var data = result.Buffer;
            try
            {
                var remoteEP = result.RemoteEndPoint;
                //var data = socket.EndReceive(result, ref remoteEP);
                string msg = Encoding.ASCII.GetString(data);
                handler.HandleMessage(msg);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Disconnect();
            }
        }
    }
    public void Disconnect()
    {
        Debug.Log("UDP disconnected");
        socket.Close();
        socket = null;
    }
}
