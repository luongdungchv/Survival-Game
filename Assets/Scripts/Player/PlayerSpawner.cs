using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private GameObject markerFullMap, markerMinimap;
    void Start()
    {
        var randObj = new CustomRandom(MapGenerator.ins.seed);
        var castPos = new Vector3(randObj.NextFloat(100, 1400), 100, randObj.NextFloat(100, 1400));

        var terrainTypes = MapGenerator.ins.terrainTypes;
        var maxHeight = MapGenerator.ins.vertMaxHeight;
        var waterHeight = terrainTypes[terrainTypes.Count - 2].height + 0.05f;
        var skipHeight = waterHeight * maxHeight;

        RaycastHit hit;
        bool cast = Physics.Raycast(castPos + Vector3.right * int.Parse(Client.ins.clientId) * 5, Vector3.down, out hit, 100, mask);
        while (hit.collider.tag == "Water")
        {
            castPos = new Vector3(randObj.NextFloat(100, 1400) + int.Parse(Client.ins.clientId) * 5, 100, randObj.NextFloat(100, 1400));
            cast = Physics.Raycast(castPos, Vector3.down, out hit, 100, mask);
        }


        transform.position = hit.point + Vector3.up * 3;
        var pos = transform.position;

        GetComponent<NetworkPlayer>().id = Client.ins.clientId;
        GetComponent<NetworkPlayer>().port = Client.ins.udp.port;
        NetworkManager.ins.AddPlayer(Client.ins.clientId, GetComponent<NetworkPlayer>());
        if (!Client.ins.isHost)
        {
            var moveSystem = GetComponent<PlayerMovement>();
        }
        SetMarkerColor(int.Parse(Client.ins.clientId));
        Client.ins.SendTCPMessage($"{(int)PacketType.SpawnPlayer} {Client.ins.clientId} {pos.x} {pos.y} {pos.z} {Client.ins.udp.port}");
        var spawnPacket = new SpawnPlayerPacket();
        spawnPacket.WriteData(Client.ins.clientId, new Vector3(pos.x, pos.y, pos.z));

    }

    public void SetMarkerColor(int id){
        var color = GameFunctions.GetMarkerColor(id);
        markerFullMap.GetComponent<Renderer>().material.SetColor("_Color", color);
        markerMinimap.GetComponent<Renderer>().material.SetColor("_Color", color);
    }
}
