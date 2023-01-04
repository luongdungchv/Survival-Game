using UnityEngine;
using UnityEngine.PlayerLoop;

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
            //placeHolder.transform.rotation = rotateToSlope;

            var slopeTangent = Vector3.Cross(hit.normal, player.transform.forward);
            var placerForward = Vector3.Cross(hit.normal, slopeTangent);
            Debug.DrawLine(hit.point + hit.normal, hit.point + hit.normal + placerForward * 10, Color.magenta);
            //Debug.Log(placerForward);

            placeHolder.transform.LookAt(placeHolder.transform.position + placerForward, hit.normal);
            //placeHolder.transform.Rotate(0, 180, 0);
            //placeHolder.transform.LookAt();
        }
    }
    public void ConfirmPosition()
    {
        var id = Client.ins.clientId;
        var netPrefab = prefab.GetComponent<NetworkPrefab>();
        var rotation = placeHolder.transform.rotation.eulerAngles;
        NetworkManager.ins.SpawnRequest(id, netPrefab, placeHolder.transform.position, rotation);
        if (Client.ins.isHost)
        {
            var obj = Instantiate(prefab, placeHolder.transform.position, placeHolder.transform.rotation);
        }
    }
    public void SetData(Color tint, Mesh mesh, Texture2D colorTex, GameObject prefab)
    {
        // this.prefab = prefab;
        // var phRenderer = placeHolder.GetComponent<Renderer>();
        // var phFilter = placeHolder.GetComponent<MeshFilter>();

        // phRenderer.sharedMaterial.mainTexture = colorTex;
        // phRenderer.sharedMaterial.SetColor("_Color", tint);
        // phFilter.mesh = mesh;

        //Debug.Log(phRenderer);

    }
}