using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System.Reflection;
using System;
using System.Linq;

namespace DL.StateMachine
{public class CustomEdgeGUI : EdgeGUI
{
    private HashSet<(Node, Node)> mapper = new HashSet<(Node, Node)>();
    private float arrowSize = 5f;
    private List<CustomEdge> selectedEdgeList = new List<CustomEdge>();

    public List<CustomEdge> SelectedEdges => this.selectedEdgeList;

    public void RenderEdges(out bool clickedOnEdge)
    {
        var edgeList = ((this.host as StateMachineGraph).graph as CustomGraph).FSMEdgeList;
        clickedOnEdge = false;
        foreach (var edge in edgeList)
        {
            var start = edge.StartNode.position.center;
            if (edge.EndNode == null && TransitionMaker.IsMakingTransition)
            {
                var endPosition = Event.current.mousePosition;

                Handles.DrawLine(start, endPosition);
                edge.startPos = start;
                edge.endPos = endPosition;
                this.RenderArrow(start, endPosition);
                continue;
            }
            var end = edge.EndNode.position.center;
            if (mapper.Contains((edge.StartNode, edge.EndNode)))
            {
                edge.startPos = start;
                edge.endPos = end;
                Handles.DrawLine(start, end);
                this.RenderArrow(start, end);
                if (Event.current.type == EventType.MouseDown)
                {
                    clickedOnEdge = this.SelectEdge(edge);
                }
                continue;
            };
            if (mapper.Contains((edge.EndNode, edge.StartNode)))
            {
                var dir = (end - start).normalized;
                var normal = new Vector2(-dir.y, dir.x);
                start += normal * 15;
                end += normal * 15;
            }
            else
            {
                mapper.Add((edge.StartNode, edge.EndNode));
            }
            this.RenderArrow(start, end);
            edge.startPos = start;
            edge.endPos = end;
            Handles.DrawLine(start, end);
            if (Event.current.type == EventType.MouseDown)
            {
                clickedOnEdge = this.SelectEdge(edge);
            }
        }
        if (Event.current.type == EventType.MouseMove)
            Event.current.Use();
    }
    public bool SelectEdge(CustomEdge edge)
    {
        Debug.Log(edge);
        var edgeList = ((this.host as StateMachineGraph).graph as CustomGraph).FSMEdgeList;
        var mousePos = (Event.current.mousePosition);
        selectedEdgeList.Clear();

        var start = edge.startPos;
        var end = edge.endPos;
        var center = (end + start) / 2;
        var dir = (end - start).normalized;
        var edgeLength = (end - start).magnitude / 2;
        var normal = new Vector2(-dir.y, dir.x);
        var dirToMouse = (mousePos - center);

        var vertAngle = Vector3.Angle(dirToMouse, dir);
        dir = vertAngle > 90 ? -dir : dir;
        vertAngle = vertAngle > 90 ? 180 - vertAngle : vertAngle;
        var horiAngle = Vector3.Angle(dirToMouse, normal);
        normal = horiAngle > 90 ? -normal : normal;
        horiAngle = horiAngle > 90 ? 180 - horiAngle : horiAngle;

        var lengthY = dirToMouse.magnitude * Mathf.Cos(vertAngle * Mathf.Deg2Rad);
        var lengthX = dirToMouse.magnitude * Mathf.Cos(horiAngle * Mathf.Deg2Rad);



        var condition = lengthY < edgeLength && lengthX < 8;
        if (condition)
        {
            this.selectedEdgeList.Clear();
            this.selectedEdgeList.Add(edge);
            this.UpdateUnitySelection();
            Event.current.Use();
        }
        Debug.Log((lengthX, lengthY, mousePos, center, condition));
        return condition;
    }

    private void UpdateUnitySelection()
    {
        var list = this.selectedEdgeList;
        //Selection.objects = list.ToArray();
        Selection.activeObject = selectedEdgeList[0];
        Debug.Log(Selection.activeObject);
        GUIUtility.keyboardControl = 0;
        //EditorGUI.EndEditingActiveTextField();
    }
    private void RenderArrow(Vector2 start, Vector2 end)
    {
        if (Event.current.type == EventType.Repaint)
        {
            var center = (start + end) / 2;
            var direction = (end - start).normalized;
            var color = Color.white;
            var cross = new Vector2(-direction.y, direction.x);
            cross = cross.normalized;

            Vector3[] array = new Vector3[4];
            array[0] = center + direction * arrowSize;
            array[1] = center - direction * arrowSize + cross * arrowSize;
            array[2] = center - direction * arrowSize - cross * arrowSize;
            array[3] = array[0];
            Shader.SetGlobalColor("_HandleColor", color);
            //typeof(HandleUtility).GetMethod("ApplyWireMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[0]);
            typeof(HandleUtility).InvokeMember("ApplyWireMaterial", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[0]);
            //HandleUtility.ApplyWireMaterial();
            GL.Begin(4);
            GL.Color(color);
            GL.Vertex(array[0]);
            GL.Vertex(array[1]);
            GL.Vertex(array[2]);
            GL.End();
            Handles.color = color;
            Handles.DrawAAPolyLine((Texture2D)Styles.connectionTexture.image, 1, array);
        }
    }

}
}