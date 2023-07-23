
using System;
using System.Collections.Generic;
using System.Reflection;

public interface IEntityTypeTreeNode
{
    Type EntityType { get; }
    IEntityMeta Meta { get; }
    IEntityTypeTreeNode Parent { get; }
    List<IEntityTypeTreeNode> Children { get; }
    void RemoveEntity(Entity e);
    void AddEntity(Entity e);
    void Propagate(IEntityTypeTreeNotice n);
    void BubbleUp(IEntityTypeTreeNotice notice);
    void BubbleDown(IEntityTypeTreeNotice notice);
    void PushDown(IEntityTypeTreeNotice n);
    void SetParent(IEntityTypeTreeNode parent);
    
    IReadOnlyCollection<Entity> GetEntities();
    RefAction<EntityCreatedNotice> Created { get; }    
    RefAction<EntityDestroyedNotice> Destroyed { get; }
    public static IEntityTypeTreeNode ConstructFromType(Type type)
    {
        return (IEntityTypeTreeNode)typeof(EntityTypeTreeNode<>)
            .MakeGenericType(type)
            .GetMethod(nameof(EntityTypeTreeNode<Entity>.Construct), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new object?[]{});
    }
}