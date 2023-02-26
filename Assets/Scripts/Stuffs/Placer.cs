using UnityEngine;

public class Placer : MonoBehaviour
{
    [SerializeField] private GameObject placeHolder, player, prefab;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float placingHeight;

    private void Update()
    {
        var cast = Physics.Raycast(transform.position, Vector3.down, out var hit, 7, mask);
        if (cast)
        {
            placeHolder.transform.position = hit.point + hit.normal * placingHeight;
            var rotateToSlope = Quaternion.FromToRotation(Vector3.up, hit.normal);

            var slopeTangent = Vector3.Cross(hit.normal, player.transform.forward);
            var placerForward = Vector3.Cross(hit.normal, slopeTangent);
            Debug.DrawLine(hit.point + hit.normal, hit.point + hit.normal + placerForward * 10, Color.magenta);

            placeHolder.transform.LookAt(placeHolder.transform.position + placerForward, hit.normal);
        }
    }
    public void ConfirmPosition()
    {
        var id = Client.ins.clientId;
        var netPrefab = prefab.GetComponent<NetworkPrefab>();
        var rotation = placeHolder.transform.rotation.eulerAngles;

        if (Client.ins.isHost)
        {
            var obj = Instantiate(prefab, placeHolder.transform.position, placeHolder.transform.rotation);
            var objId = obj.GetComponent<NetworkSceneObject>().GenerateId();
            NetworkManager.ins.test = obj.gameObject;
            NetworkManager.ins.SpawnRequest(id, netPrefab, placeHolder.transform.position, rotation, objId);
        }
        else
        {
            NetworkManager.ins.SpawnRequest(id, netPrefab, placeHolder.transform.position, rotation, "0");
        }
    }

}