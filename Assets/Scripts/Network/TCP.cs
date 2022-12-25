using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

public class TCP
{
    private TcpClient socket;
    private NetworkStream stream;
    private byte[] buffer;
    private int bufferSize;
    private Client owner;
    private ClientHandle handler => owner.handler;
    public TCP(Client owner, int bufferSize)
    {
        this.bufferSize = bufferSize;
        this.owner = owner;
    }
    public async void Connect(string hostName, int port)
    {
        Debug.Log($"connect {hostName} {port}");
        socket = new TcpClient();
        try
        {
            await socket.ConnectAsync(hostName, port);
            Debug.Log(socket.Connected);
            stream = socket.GetStream();
            buffer = new byte[bufferSize];
            TCPReadAsync();
        }
        catch (Exception e)
        {
            Debug.Log("Cannot connect to server");
            Debug.Log(e.ToString());
        }

    }
    public void Disconnect()
    {
        socket.Close();
        socket = null;
        stream = null;
        buffer = null;
        bufferSize = 0;
    }
    public bool Send(string msg)
    {
        if (stream.CanWrite)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            stream.BeginWrite(data, 0, data.Length, null, null);
            return true;
        }
        else
        {
            Debug.Log("Cannot send");
            return false;
        }
    }
    private async void TCPReadAsync()
    {
        while (true)
        {
            try
            {
                int dataLength = await stream.ReadAsync(buffer, 0, bufferSize);
                if (dataLength <= 0)
                {
                    Disconnect();
                    return;
                }
                byte[] data = new byte[dataLength];
                Array.Copy(buffer, data, dataLength);
                string msg = Encoding.ASCII.GetString(data);
                handler.HandleMessage(msg);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Disconnect();
                break;
            }
        }
    }
    private void HandleTCPMessage(string msg)
    {
        var split = msg.Split(' ');
        string cmd = split[0];
        Debug.Log(msg);
        if (cmd == "udp")
        {
            owner.SetUDPRemoteHost(int.Parse(split[1]));
            owner.SendUDPMessage("con");
            owner.StartCoroutine(LoadSceneDelay(2));
            owner.clientId = split[2];
            owner.mapSeed = int.Parse(split[3]);
        }
    }
    IEnumerator LoadSceneDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("Test_PlayerStats");
    }
}
