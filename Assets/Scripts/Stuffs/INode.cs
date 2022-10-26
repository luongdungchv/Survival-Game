using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public interface INode
{
    BehaviourTree bht { get; set; }
    NodeState state { get; set; }
    NodeState Evaluate();
}
public interface IParentNode
{
    void AddChild(INode child);
    List<INode> GetChildren();
}
public enum NodeState
{
    Running, Failed, Success, Unidentified
}
public class Sequence : INode, IParentNode
{
    private NodeState _state;
    public NodeState state { get => _state; set => _state = value; }
    public BehaviourTree bht { get; set; }

    public List<INode> children;
    public Sequence(BehaviourTree bht)
    {
        this.bht = bht;
        _state = NodeState.Unidentified;
        children = new List<INode>();
    }

    public NodeState Evaluate()
    {
        if (children.Count == 0) return NodeState.Unidentified;
        foreach (var i in children)
        {
            if (i.Evaluate() == NodeState.Failed) return NodeState.Failed;
            else if (i.Evaluate() == NodeState.Running) return NodeState.Running;
        }
        return NodeState.Success;
    }

    public void AddChild(INode child)
    {
        children.Add(child);
    }

    public List<INode> GetChildren()
    {
        return children;
    }
}
public class Selector : INode, IParentNode
{

    private NodeState _state;
    public NodeState state { get => _state; set => _state = value; }
    public List<INode> children;
    public BehaviourTree bht { get; set; }
    public Selector(BehaviourTree bht)
    {
        this.bht = bht;
        _state = NodeState.Unidentified;
        children = new List<INode>();
    }

    public NodeState Evaluate()
    {
        if (children.Count == 0) return NodeState.Unidentified;

        foreach (var i in children)
        {
            var childState = i.Evaluate();
            if (childState == NodeState.Success || childState == NodeState.Running)
            {
                this._state = childState;
                return childState;
            };
        }
        return NodeState.Failed;
    }

    public void AddChild(INode child)
    {
        children.Add(child);
    }

    public List<INode> GetChildren()
    {
        return children;
    }
}
public class RandomSelector : INode, IParentNode
{
    private NodeState _state;
    public BehaviourTree bht { get; set; }
    public NodeState state { get => _state; set => _state = value; }
    public List<INode> children;
    private int runningIndex;
    public RandomSelector(BehaviourTree bht)
    {
        this.bht = bht;
        _state = NodeState.Unidentified;
        children = new List<INode>();
    }

    public NodeState Evaluate()
    {
        if (children.Count == 0) return NodeState.Unidentified;
        if (this._state != NodeState.Running)
        {
            int randIndex = UnityEngine.Random.Range(0, children.Count);
            this._state = children[randIndex].Evaluate();
            runningIndex = randIndex;
        }
        else
        {
            var childState = children[runningIndex].state;
            if (childState == NodeState.Success || childState == NodeState.Unidentified)
            {
                int randIndex = UnityEngine.Random.Range(0, children.Count);
                this._state = children[randIndex].Evaluate();
                runningIndex = randIndex;
            }
        }
        return this._state;
    }

    public void AddChild(INode child)
    {
        children.Add(child);
    }

    public List<INode> GetChildren()
    {
        return children;
    }
}
public class TaskNode : INode
{
    private NodeState _state = NodeState.Unidentified;
    public BehaviourTree bht { get; set; }
    public NodeState state { get => _state; set => _state = value; }
    private UnityAction<TaskNode> task;

    public TaskNode(BehaviourTree bht, string nodeId)
    {
        this.bht = bht;
        this.task = this.bht.GetTask(nodeId);
        _state = NodeState.Unidentified;


    }

    public NodeState Evaluate()
    {
        if (this._state == NodeState.Running) return this._state;
        task(this);
        return this._state;
    }
    public void TaskStart()
    {
        this._state = NodeState.Running;
    }
    public void TaskComplete()
    {
        this._state = NodeState.Success;
    }
    public void TaskFail()
    {
        this._state = NodeState.Failed;
    }
}
public class ConditionNode : INode
{
    public BehaviourTree bht { get; set; }
    public NodeState state { get; set; }
    Func<ConditionNode, bool> condition;

    public ConditionNode(BehaviourTree bht, string id)
    {
        this.bht = bht;
        this.condition = this.bht.GetCondition(id);
        state = NodeState.Unidentified;

    }
    public NodeState Evaluate()
    {
        var check = condition(this);
        return check ? NodeState.Success : NodeState.Failed;
    }
}
