using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateComponent
{
    void OnStateEnter(StateInitializer init);
    void OnStateUpdate(StateInitializer init);
    void OnStateExit(StateInitializer init);
    
}
