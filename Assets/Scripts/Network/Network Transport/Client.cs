using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Client : MonoBehaviour
{
    public static Client ins;
    public UnityEvent<string> OnTCPMessageReceive;

    public UnityEvent<string> OnUDPMessageReceive;
    public bool isHost;
    public int mapSeed;
    public string clientId;
    public ClientHandle handler;
    [SerializeField] private string server;
    [SerializeField] private int port, tcpBufferSize;
    [SerializeField] Button joinRoomBtn;
    [SerializeField] private TMP_InputField field;
    public TCP tcp;
    public UDP udp;
    public string hostName => server;
    private void Awake()
    {
        if (ins == null) ins = this;
        Debug.Log(NetworkPrefab.instanceCount);
        field.text = "12345";
    }

    void Start()
    {
        tcp = new TCP(this, tcpBufferSize);
        udp = new UDP(this);
        DontDestroyOnLoad(this.gameObject);
        joinRoomBtn.onClick.AddListener(() => JoinRoom(field.text));
        Application.runInBackground = true;
    }
    private void Update()
    {

    }
    public void SendTCPMessage(string msg)
    {
        tcp.Send(msg);
    }
    public void SendTCPPacket(Packet _packet)
    {
        tcp.Send(_packet.GetString());
    }
    public void SendUDPMessage(string msg)
    {
        udp.Send(msg);
    }
    public void SendUDPPacket(Packet _packet)
    {
        udp.Send(_packet.GetString());
    }
    public void CreateRoom()
    {
        if (tcp.Send("cr"))
        {
            isHost = true;
        }
    }
    public void JoinRoom(string id)
    {
        tcp.Send($"jr {id}");
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
    public void ConnectToServer()
    {
        tcp.Connect(server, port);
    }
    public void SetUDPRemoteHost(int port)
    {
        udp.Connect(server, port);
    }
    private void OnApplicationQuit()
    {
        tcp.Disconnect();
        udp.Disconnect();
    }
}
