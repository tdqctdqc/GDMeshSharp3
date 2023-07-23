using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityTypeTree
{
    public Dictionary<Type, EntityTypeTreeNode> Nodes { get; private set; }
    public EntityTypeTree(Data data)
    {
        Nodes = new Dictionary<Type, EntityTypeTreeNode>();
    }
    public EntityTypeTreeNode Get<T>() where T : Entity
    {
        return Get(typeof(T));
    }
    public EntityTypeTreeNode Get(Type type) 
    {
        if(Nodes.ContainsKey(type) == false) Add(type);
        return Nodes[type];
    }
    private void Add(Type type)
    {
        var node = new EntityTypeTreeNode(type);
        Nodes.Add(type, node);
        var parentType = type.BaseType;
        if (Nodes.ContainsKey(parentType) == false && typeof(Entity).IsAssignableFrom(parentType))
        {
            Add(parentType);
        }
        if(Nodes.TryGetValue(parentType, out var pNode))
        {
            node.SetParent(Nodes[parentType]);
        }
        foreach (var type1 in Nodes.Keys.ToList())
        {
            if (type1.BaseType == type)
            {
                Nodes[type1].SetParent(node);
            }
        }
    }
}
