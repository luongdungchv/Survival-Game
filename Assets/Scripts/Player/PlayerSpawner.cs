using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    void Start()
    {
        var randObj = new CustomRandom(MapGenerator.ins.seed + int.Parse(Client.ins.clientId));
        var castPos = new Vector3(randObj.NextFloat(100, 1400), 100, randObj.NextFloat(100, 1400));

        var terrainTypes = MapGenerator.ins.terrainTypes;
        var maxHeight = MapGenerator.ins.vertMaxHeight;
        var waterHeight = terrainTypes[terrainTypes.Count - 2].height + 0.05f;
        var skipHeight = waterHeight * maxHeight;

        RaycastHit hit;
        bool cast = Physics.Raycast(castPos, Vector3.down, out hit, 100, mask);
        cast = !cast || (cast && hit.point.y < skipHeight) ? false : true;
        while (!cast)
        {
            castPos = new Vector3(randObj.NextFloat(100, 1400), 100, randObj.NextFloat(100, 1400));
            cast = Physics.Raycast(castPos, Vector3.down, out hit, 100, mask);
            cast = !cast || (cast && hit.point.y < skipHeight) ? false : true;
        }


        transform.position = hit.point + Vector3.up;
        var pos = transform.position;

        GetComponent<NetworkPlayer>().id = Client.ins.clientId;
        NetworkManager.ins.AddPlayer(Client.ins.clientId, GetComponent<NetworkPlayer>());
        if (!Client.ins.isHost)
        {
            GetComponent<Rigidbody>().useGravity = false;
        }
        Client.ins.SendTCPMessage($"{(int)PacketType.SpawnPlayer} {Client.ins.clientId} {pos.x} {pos.y} {pos.z}");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
