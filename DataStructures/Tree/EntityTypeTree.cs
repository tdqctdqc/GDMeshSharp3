using System;
using System.Collections.Generic;
using System.Linq;

public class EntityTypeTree
{
    public EntityTypeTreeNode this[Type type] => _nodes[type];
    private Dictionary<Type, EntityTypeTreeNode> _nodes;
    private HashSet<Type> _rootTypes;
    public EntityTypeTree(IEnumerable<Type> types)
    {
        _nodes = new Dictionary<Type, EntityTypeTreeNode>();
        foreach (var type in types)
        {
            _nodes.Add(type, new EntityTypeTreeNode(type));
        }
        _rootTypes = new HashSet<Type>();
        foreach (var node in _nodes.Values)
        {
            var parentType = node.EntityType.BaseType;
            if(_nodes.TryGetValue(parentType, out var pNode))
            {
                node.SetParent(pNode);
            }
            else
            {
                _rootTypes.Add(node.EntityType);
            }
        }
    }
}
