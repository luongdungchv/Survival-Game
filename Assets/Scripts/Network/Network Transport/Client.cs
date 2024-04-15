using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public static Client ins;
    public UnityEvent<string> OnTCPMessageReceive;

    public UnityEvent<string> OnUDPMessageReceive;
    public bool isHost;
    public int mapSeed;
    public string clientId;
    public string clientName;
    public ClientHandle handler;
    [SerializeField] private string server;
    [SerializeField] private int port, tcpBufferSize;
    [SerializeField] Button joinRoomBtn;
    [SerializeField] private TMP_InputField roomField, hostField;
    public TCP tcp;
    public UDP udp;
    public string hostName => server;
    public Mesh testmesh;
    public bool isSinglePlayer = false;
    private void Awake()
    {
        //Screen.SetResolution(960, 540, FullScreenMode.Windowed);
        if (ins == null) ins = this;
        else
        {
            Destroy(ins.gameObject);
            ins = this;
        }
        roomField.text = "12345";
        
        hostField.onValueChanged.AddListener(e => {
            server = e;
        }); 
        hostField.text = server;
    }

    void Start()
    {
        tcp = new TCP(this, tcpBufferSize);
        udp = new UDP(this);
        DontDestroyOnLoad(this.gameObject);
        joinRoomBtn.onClick.AddListener(() => JoinRoom(roomField.text));
        Application.runInBackground = true;
    }
    public void SendTCPMessage(string msg)
    {
        if(isSinglePlayer) return;
        tcp.Send(msg);
    }
    public void SendTCPPacket(Packet _packet)
    {
        if(isSinglePlayer) return;
        tcp.Send(_packet.GetString());
    }
    public void SendUDPMessage(string msg)
    {
        if(isSinglePlayer) return;
        udp.Send(msg);
    }
    public void SendUDPPacket(Packet _packet)
    {
        if(isSinglePlayer) return;
        udp.Send(_packet.GetString());
    }
    public void SendUDPConnectionInfo(Action sendCompleteCallback)
    {
        if(isSinglePlayer) return;
        tcp.Send($"con {this.udp.GetSocketEP().Port}", sendCompleteCallback);
    }
    public void CreateRoom()
    {
        if (!tcp.isConnected)
        {
            tcp.Connect(server, port, () =>
            {
                if (tcp.Send("cr " + mapSeed.ToString()))
                {
                    isHost = true;
                }
            });
        }
        else
        {
            if (tcp.Send("cr " + mapSeed.ToString()))
            {
                isHost = true;
            }
        }
    }
    public void JoinRoom(string id)
    {
        if (!tcp.isConnected)
        {
            tcp.Connect(server, port, () =>
            {
                tcp.Send($"jr {id}");
            });
        }
        else
        {
            tcp.Send($"jr {id}");
        }
    }
    public void SinglePlayer(){
        clientId = "0";
        isSinglePlayer = true;
        SceneManager.LoadScene("Test_PlayerStats");
    }
    public void Ready()
    {
        tcp.Send("rd");
    }
    public void StartGame()
    {
        if (isHost)
        {
            tcp.Send($"st {mapSeed}");
        }
    }
    public void LeaveRoom()
    {
        tcp.Send("lv");
    }
    public void ConnectToServer()
    {
        tcp.Connect(server, port);
    }
    public void SetUDPRemoteHost(int port)
    {
        udp.Connect(server, port);
    }
    public void LeaveGame(){
        tcp.Disconnect();
        udp.Disconnect();
    }
    private void OnApplicationQuit()
    {
        LeaveGame();
    }
}
