using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private bool syncRotation;
    void Start()
    {

    }

    void Update()
    {
        transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
        if (syncRotation)
        {
            var playerRotation = player.rotation.eulerAngles;
            var currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, playerRotation.y, currentRotation.z);
        }
    }
}
