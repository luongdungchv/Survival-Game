using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 movementInputVector;
    public bool sprint;
    public bool jumpPress;
    private NetworkObject netObj;
    void Start()
    {

    }
    public void HandleInput(InputPacket _packet)
    {
        this.movementInputVector = _packet.inputVector;
        this.sprint = _packet.sprint;
        this.jumpPress = _packet.jump;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
