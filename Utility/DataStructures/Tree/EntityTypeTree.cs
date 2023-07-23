using System;
using System.Collections.Generic;
using System.Linq;

public class EntityTypeTree
{
    public EntityTypeTreeNode this[Type type] => Nodes[type];
    public Dictionary<Type, EntityTypeTreeNode> Nodes { get; private set; }
    public EntityTypeTree(Data data)
    {
        Nodes = new Dictionary<Type, EntityTypeTreeNode>();
    }

    public EntityTypeTreeNode Get(Type type, Data data) 
    {
        if(Nodes.ContainsKey(type) == false) Add(type, data);
        return Nodes[type];
    }
    private void Add(Type type, Data data)
    {
        var node = new EntityTypeTreeNode(type, data);
        Nodes.Add(type, node);
        var parentType = type.BaseType;
        if(Nodes.TryGetValue(parentType, out var pNode))
        {
            node.SetParent(pNode);
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
