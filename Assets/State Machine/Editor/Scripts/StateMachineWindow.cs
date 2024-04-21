using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.Graphs;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.CodeDom;
using System;
using System.Reflection;

namespace DL.StateMachine.Editor
{
    public class StateMachineWindow : EditorWindow
    {
        private StateMachineGraph graphGUI;
        private CustomGraph graph;

        public CustomGraph GraphModel => this.graph;
        public StateMachineGraph GraphGUI => this.graphGUI;

        public UnityAction OnUpdate;

        private IMGUIContainer guiContainer;
        private Rect windowArea;
        private Manipulator manipulator;
        private StateMachineDataSO dataHolder;

        private float zoomLevel = 1;
        public Vector2 windowPosition => this.manipulator.position;

        [OnOpenAsset(0)]
        public static bool ShowWindow(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);
            if (asset is StateMachineDataSO)
            {
                var window = GetWindow<StateMachineWindow>("Test");
                window.wantsMouseMove = true;
                window.wantsMouseEnterLeaveWindow = true;
                window.Show();
                window.SetUpGraph();
                window.SetDataHolder(asset as StateMachineDataSO);
                window.LoadData(true);
            }


            return false;
        }

        public void SetDataHolder(StateMachineDataSO dataHolder)
        {
            this.dataHolder = dataHolder;
        }

        public void SetUpGraph()
        {
            graph = ScriptableObject.CreateInstance<CustomGraph>();
            graph.hideFlags = HideFlags.HideAndDontSave;

            // CustomNode node = ScriptableObject.CreateInstance<CustomNode>();
            // node.title = "mile";
            // node.position = new Rect(2500, 2500, 0, 0);

            // var node2 = ScriptableObject.CreateInstance<CustomNode>();
            // node2.title = "node2";
            // node2.position = new Rect(2580, 2580, 0, 0);

            // graph.AddNode(node);
            // graph.AddNode(node2);

            // var node3 = ScriptableObject.CreateInstance<CustomNode>();
            // node3.title = "node2";
            // node3.position = new Rect(2660, 2660, 0, 0);



            graphGUI = ScriptableObject.CreateInstance<StateMachineGraph>();
            graphGUI.graph = graph;

            this.windowArea = new Rect(-2500, -2500, 5000, 5000);
            Debug.Log(windowArea);
            this.manipulator = new Manipulator();
            this.manipulator.SetUp(windowArea, this);
            this.zoomLevel = 1;

            // this.AddNewNode(2500, 2500);
            // this.AddNewNode(2580, 2580);
            // this.AddNewNode(2660, 2660);
        }

        public CustomNode AddNewNode(float x, float y, string title = "New Node")
        {
            var position = new Vector2(x, y);
            CustomNode node = ScriptableObject.CreateInstance<CustomNode>();
            node.title = title;
            node.position = new Rect(position.x, position.y, 0, 0);
            graph.AddNode(node);
            return node;
        }
        public CustomNode AddNewNode(Vector2 position, string title = "New Node")
        {
            return this.AddNewNode(position.x, position.y, title);
        }

        private void OnBecameVisible()
        {
            this.LoadData();
        }

        private void OnGUI()
        {
            if (graphGUI != null)
            {
                //Debug.Log(this.position);
                graphGUI.BeginGraphGUI(this, new Rect(0, 0, this.position.width, this.position.height));
                GUI.EndScrollView();
                manipulator.BeginGUI();
                this.HandlePan();
                graphGUI.OnGraphGUI();
                graphGUI.EndGraphGUI();
                manipulator.EndGUI();

                if (GUI.Button(new Rect(0, 21, 75, 20), "Save"))
                {
                    this.SaveData();
                }

                graphGUI.DisplayContextMenu();
                this.HandleScroll();
            }
        }
        private void HandleScroll()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                var zoomAmount = Event.current.delta.y / Math.Abs(Event.current.delta.y);
                var lastZoomLevel = zoomLevel;
                this.zoomLevel -= zoomAmount / 8;
                zoomLevel = Mathf.Clamp(zoomLevel, 0.25f, 2.25f);

                var ratio = zoomLevel / lastZoomLevel;

                manipulator.SetZoomScale(zoomLevel);
                var dir = manipulator.position - Event.current.mousePosition;
                dir *= ratio;
                manipulator.SetRectPosition(Event.current.mousePosition + dir);

                Event.current.Use();
            }
        }
        private void HandlePan()
        {

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
            {
                Debug.Log("Drag");
                var delta = Event.current.delta * zoomLevel;
                this.manipulator.Translate(delta);
                Event.current.Use();

            }
        }

        public void SaveData()
        {
            this.dataHolder.ResetData();
            var index = 0;
            var nodeToIndexMap = new Dictionary<CustomNode, int>();
            foreach (var n in this.graph.nodes)
            {
                var node = n as CustomNode;
                var state = new StateHolder()
                {
                    name = node.title,
                    index = index,
                    scriptAssetList = node.ScriptAssets,
                    positionInGraph = node.position
                };
                this.dataHolder.stateList.Add(state);
                nodeToIndexMap.Add(node, index);
                index++;
            }
            foreach (var e in (this.graph as CustomGraph).FSMEdgeList)
            {
                var edge = e as CustomEdge;
                if (edge.EndNode == null) continue;
                var startIndex = nodeToIndexMap[edge.StartNode as CustomNode];
                var endIndex = nodeToIndexMap[edge.EndNode as CustomNode];
                var stateTranistion = new StateTransition()
                {
                    startIndex = startIndex,
                    endIndex = endIndex
                };
                dataHolder.stateTransitionList.Add(stateTranistion);
            }
            this.dataHolder.zoomLevel = this.zoomLevel;
            this.dataHolder.camPostion = this.windowPosition;
            EditorUtility.SetDirty(this.dataHolder);
            AssetDatabase.SaveAssets();
        }
        public void LoadData(bool adjustCamPos = false)
        {
            if (this.dataHolder == null) return;
            var nodeIndexMap = new Dictionary<int, CustomNode>();
            this.graph.nodes.Clear();
            (this.graph as CustomGraph).FSMEdgeList.Clear();
            Debug.Log("Loading data");
            foreach (var state in this.dataHolder.stateList)
            {
                var addedNode = this.AddNewNode(state.positionInGraph.position, state.name);
                addedNode.SetScriptAssets(state.scriptAssetList);
                nodeIndexMap[state.index] = addedNode;
            }
            foreach (var transition in this.dataHolder.stateTransitionList)
            {
                var startNode = nodeIndexMap[transition.startIndex];
                var endNode = nodeIndexMap[transition.endIndex];
                this.graph.Connect(startNode, endNode);
            }
            if (adjustCamPos)
            {
                this.zoomLevel = this.dataHolder.zoomLevel;
                manipulator.SetZoomScale(zoomLevel);
                this.manipulator.SetRectPosition(this.dataHolder.camPostion);
            }
        }

        public Vector2 WindowToGroupPosition(Vector2 pos)
        {
            pos -= this.windowPosition;
            pos /= zoomLevel;
            Debug.Log(this.windowPosition);
            return pos;
        }
        // private void Update() {
        //     this.graphGUI.OnUpdate();
        // }
    }
}