using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class NodeList<TNode, TEdge> 
{
    private Dictionary<TNode, GraphNode<TNode, TEdge>> _nodesByValue;
    public int Count => _nodesByValue.Count;
    public NodeList() : base() 
    {
        _nodesByValue = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
    }

    public List<GraphNode<TNode, TEdge>> GetNodes()
    {
        return _nodesByValue.Values.ToList();
    }
    public bool ContainsValue(TNode t)
    {
        return _nodesByValue.ContainsKey(t);
    }
    public new void AddNode(GraphNode<TNode, TEdge> item) 
    {
        _nodesByValue.Add(item.Element, item);
    }
    
    public void Remove(TNode t)
    {
        _nodesByValue.Remove(t);
    }
    public GraphNode<TNode, TEdge> FindByValue(TNode value)
    {
        return _nodesByValue[value];
    }
}
