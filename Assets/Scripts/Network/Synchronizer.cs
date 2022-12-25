using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.PostProcessing;

public class Synchronizer : MonoBehaviour
{
    private Client client => Client.ins;
    [SerializeField] private float syncRate, delay;
    public bool isLocalPlayer;
    public string id;
    private float elapsed;
    private NetworkObject netObj;
    private Rigidbody rb;
    private void Start()
    {
        //netObj = GetComponent<NetworkObject>();
        string lastMsg = "";
        var lastPos = transform.position;
        rb = GetComponent<Rigidbody>();
        client.OnUDPMessageReceive.AddListener(msg =>
        {
            var split = msg.Split(' ');
            if (id == "1" && split[1] == id && msg != lastMsg)
            {
                lastMsg = msg;
                Debug.Log(msg);
            }
            if (split[0] == "p" && split[1] == id && !client.isHost)
            {
                var pos = new Vector3(float.Parse(split[2]), float.Parse(split[3]), float.Parse(split[4]));
                StartCoroutine(LerpPosition(pos, delay));

            }
        });
    }
    private IEnumerator LerpPosition(Vector3 pos, float duration)
    {
        float t = 0;
        Vector3 initialPos = transform.position;
        while (t < 1)
        {
            transform.position = Vector3.Lerp(initialPos, pos, t);
            if (TryGetComponent<PlayerMovement>(out var movement))
            {
                movement.SyncCamera();
            }
            t += Time.deltaTime / duration;
            yield return null;
        }
    }
    private void Update()
    {
        if (elapsed >= syncRate && id == "1")
        {

            elapsed = 0;
        }
        if (id == "1") { SyncPosition(); }
        elapsed += Time.deltaTime;
    }
    public void SyncPosition()
    {
        if (Client.ins.isHost)
        {
            var pos = transform.position;
            client.SendUDPMessage($"p {id} {pos.x.ToString("0.0")} {pos.y.ToString("0.0")} {pos.z.ToString("0.0")}");
        }
    }
}
