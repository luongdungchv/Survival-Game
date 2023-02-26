using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEnemy : MonoBehaviour
{
    private Client client => Client.ins;
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Coroutine rotationCoroutine, lerpPosRoutine;
    
    private NetworkSceneObject netObj;
    private void Awake() {
        rb = GetComponent<Rigidbody>();
        netObj = GetComponent<NetworkSceneObject>();
    }
    private void BroadcastState(){
        var statePacket = new UpdateEnemyPacket(){
            position = transform.position,
            objId = netObj.id
        };
        client.SendUDPPacket(statePacket);
    }
    public void ReceiveStateUpdate(UpdateEnemyPacket packet){
        var _position = packet.position;
        var moveDir = _position - lastPosition;

        if (moveDir.magnitude < 0.006) moveDir = Vector3.zero;

        if (moveDir != Vector3.zero)
        {
            moveDir.y = 0;

            if (moveDir.magnitude > 0.01)
            {
                float angle = -Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg;
                float forwardAngle = -Mathf.Atan2(transform.forward.z, transform.forward.x) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle - forwardAngle) > 8)
                {
                    Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, angle + 90, transform.rotation.z);
                    rotationCoroutine = StartCoroutine(LerpRotation(transform.rotation, targetRotation, 0.1f));
                }

            }
            moveDir = moveDir.normalized;
            if (lerpPosRoutine != null) StopCoroutine(lerpPosRoutine);
            lerpPosRoutine = StartCoroutine(LerpPosition(_position, 0.0833f));
        }
        
    }
    private IEnumerator LerpPosition(Vector3 to, float duration)
    {
        Vector3 from = transform.position;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }
    IEnumerator LerpRotation(Quaternion from, Quaternion to, float duration)
    {
        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Lerp(from, to, t);
            yield return null;
        }
    }
}
