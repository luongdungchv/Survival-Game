using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

namespace DL.StateMachine.Editor
{
    public class CustomGraph : Graph
    {
        private List<CustomEdge> fsmEdgeList = new List<CustomEdge>();
        public List<CustomEdge> FSMEdgeList => fsmEdgeList;
        public void Connect(Node startNode, Node endNode)
        {
            var edge = CreateInstance<CustomEdge>();
            edge.SetStartNode(startNode);
            edge.SetEndNode(endNode);
            this.fsmEdgeList.Add(edge);
        }
        public void AddEdge(CustomEdge edge)
        {
            this.fsmEdgeList.Add(edge);
        }
        public void RemoveLastEdge()
        {
            if (this.fsmEdgeList.Count == 0) return;
            this.fsmEdgeList.RemoveAt(this.fsmEdgeList.Count - 1);
        }
        public void RemoveEdge(CustomEdge edge)
        {
            this.fsmEdgeList.Remove(edge);
            DestroyImmediate(edge);
        }

    }
}
