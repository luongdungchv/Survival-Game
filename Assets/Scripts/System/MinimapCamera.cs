using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private bool syncRotation;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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
