
using System;
using System.Collections.Generic;
using System.Reflection;

public interface IEntityTypeTreeNode
{
    Type EntityType { get; }
    IEntityTypeTreeNode Parent { get; }
    List<IEntityTypeTreeNode> Children { get; }
    HashSet<Type> ChildTypes { get; }
    void CollectChildTypes();
    void Propagate(Entity e, EntityNotice noticeType);
    void BubbleUp(Entity e, Type entityType, EntityNotice noticeType);
    void BubbleDown(Entity e, Type entityType, EntityNotice noticeType);
    void SetParent(IEntityTypeTreeNode parent);
    public static IEntityTypeTreeNode ConstructFromType(Type type)
    {
        return (IEntityTypeTreeNode)typeof(EntityTypeTreeNode<>)
            .MakeGenericType(type)
            .GetMethod(nameof(EntityTypeTreeNode<Entity>.Construct), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new object?[]{});
    }
}