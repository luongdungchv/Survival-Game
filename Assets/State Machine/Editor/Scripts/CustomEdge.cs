using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

namespace DL.StateMachine.Editor
{public class CustomEdge: ScriptableObject
{
    private Node startNode, endNode;

    public Node StartNode => this.startNode;
    public Node EndNode => this.endNode;

    public Vector2 startPos, endPos;

    public CustomEdge(Node startNode, Node endNode){
        this.startNode = startNode;
        this.endNode = endNode;
    }

    public CustomEdge(){

    }

    public void SetStartNode(Node startNode){
        this.startNode = startNode;
    }
    public void SetEndNode(Node endNode){
        this.endNode = endNode;
    }
}
}