using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

public class BehaviourTree : MonoBehaviour
{
    [SerializeField] private TextAsset xmlText;
    private Dictionary<string, UnityAction<TaskNode>> taskList = new Dictionary<string, UnityAction<TaskNode>>();
    private Dictionary<string, Func<ConditionNode, bool>> conditionList = new Dictionary<string, Func<ConditionNode, bool>>();

    public UnityEvent<string> OnTaskChange;
    public INode rootNode;
    public TaskNode runningNode;

    private void Start()
    {
        GenerateNodes();
        Traverse(rootNode);
    }
    private void Update()
    {
        Traverse(rootNode);
    }
    public void Traverse(INode node)
    {
        node.Evaluate();
    }
    private void GenerateNodes()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText.text);
        var rootElement = doc.DocumentElement.FirstChild as XmlElement;
        rootNode = CreateBHTNode(rootElement);
        ReadNodeRecursively(rootElement, rootNode);
    }
    private void ReadNodeRecursively(XmlElement parentXML, INode parent)
    {

        if (!(parent is IParentNode)) return;
        for (int i = 0; i < parentXML.ChildNodes.Count; i++)
        {
            var nodeXML = parentXML.ChildNodes[i] as XmlElement;
            var node = CreateBHTNode(nodeXML);
            (parent as IParentNode).AddChild(node);
            ReadNodeRecursively(nodeXML, node);
        }
    }
    public void SetTask(string id, UnityAction<TaskNode> task)
    {
        if (taskList.ContainsKey(id)) taskList[id] = task;
        else taskList.Add(id, task);
    }

    public UnityAction<TaskNode> GetTask(string id)
    {
        if (taskList.ContainsKey(id)) return taskList[id];
        else return null;
    }
    public void SetCondition(string id, Func<ConditionNode, bool> condition)
    {
        if (conditionList.ContainsKey(id)) conditionList[id] = condition;
        else conditionList.Add(id, condition);
    }
    public Func<ConditionNode, bool> GetCondition(string id)
    {
        if (conditionList.ContainsKey(id)) return conditionList[id];
        return null;
    }


    private INode CreateBHTNode(XmlElement xmlNode)
    {
        string type = xmlNode.Name;
        switch (type.ToLower())
        {
            case "sequence":
                return new Sequence(this);
            case "selector":
                return new Selector(this);
            case "randomselector":
                return new RandomSelector(this);
            case "task":
                return new TaskNode(this, xmlNode.GetAttribute("id"));
            case "condition":
                return new ConditionNode(this, xmlNode.GetAttribute("id"));
            default:
                return null;
        }
    }
}

