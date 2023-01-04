using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkFSM : MonoBehaviour
{
    public NetworkPlayerState currentState;
}
public enum NetworkPlayerState
{
    Move, Idle, InAir, SwimIdle, Swimming, Dash
}
