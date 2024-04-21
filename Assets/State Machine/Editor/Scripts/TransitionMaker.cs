using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Graphs;

namespace DL.StateMachine.Editor
{public class TransitionMaker
{
    public static bool IsMakingTransition;
    private Node startNode;

    public Node StartNode => this.startNode;
    public TransitionMaker(){
        IsMakingTransition = false;
    }

    public void SetStartNode(Node startNode){
        this.startNode = startNode;
    }

    public void StartMakingTransition(){
        var evt = Event.current;
        IsMakingTransition = true;     
    }
    public void StopMakingTransition(){
        this.startNode = null;
        IsMakingTransition = false;
    }

    public void OnGUI(){
        
    }
}
}