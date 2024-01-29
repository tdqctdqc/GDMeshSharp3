using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityTypeTreeNode<T> : IEntityTypeTreeNode where T : Entity
{
    public Type EntityType { get; private set; }
    public IEntityMeta Meta { get; private set; }
    public IEntityTypeTreeNode Parent { get; private set; }
    public List<IEntityTypeTreeNode> Children { get; private set; }
    public RefAction<EntityCreatedNotice> Created { get; private set; }    
    public RefAction<EntityDestroyedNotice> Destroyed { get; private set; }
    public HashSet<T> Entities { get; private set; }
    public IReadOnlyCollection<Entity> GetEntities() => Entities;

    public static EntityTypeTreeNode<T> Construct()
    {
        return new EntityTypeTreeNode<T>();
    }
    public EntityTypeTreeNode()
    {
        EntityType = typeof(T);
        Children = new List<IEntityTypeTreeNode>();
        Created = new RefAction<EntityCreatedNotice>();
        Destroyed = new RefAction<EntityDestroyedNotice>();
        Entities = new HashSet<T>();
        Meta = IEntityMeta.ConstructFromType(typeof(T));
    }
    public void Propagate(IEntityTypeTreeNotice n)
    {
        n.HandleForTreeNode(this);
        Parent?.BubbleUp(n);
        PushDown(n);
    }
    public void BubbleUp(IEntityTypeTreeNotice notice)
    {
        notice.HandleForTreeNode(this);
        Parent?.BubbleUp(notice);
    }
    public void BubbleDown(IEntityTypeTreeNotice notice)
    {
        notice.HandleForTreeNode(this);
        PushDown(notice);
    }
    public void PushDown(IEntityTypeTreeNotice notice)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            if (notice.EntityType.IsAssignableFrom(Children[i].EntityType))
            {
                Children[i].BubbleDown(notice);
                break;
            }
        }
    }
    public void AddEntity(Entity e)
    {
        Entities.Add((T) e);
    }
    public void RemoveEntity(Entity e)
    {
        Entities.Remove((T) e);
    }
    public void SetParent(IEntityTypeTreeNode parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
