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
    public IEntityTypeTreeNode Parent { get; private set; }
    public List<IEntityTypeTreeNode> Children { get; private set; }
    private Action<T> _created;    
    private Action<T> _destroyed;
    public HashSet<T> Entities { get; private set; }
    public HashSet<Type> ChildTypes { get; private set; }

    public static EntityTypeTreeNode<T> Construct()
    {
        return new EntityTypeTreeNode<T>();
    }
    public EntityTypeTreeNode()
    {
        EntityType = typeof(T);
        Children = new List<IEntityTypeTreeNode>();
        Entities = new HashSet<T>();
    }

    public void SubscribeForCreation(Action<T> action)
    {
        _created += action;
    }
    public void SubscribeForDestruction(Action<T> action)
    {
        _destroyed += action;
    }

    public void CollectChildTypes()
    {
        ChildTypes = new HashSet<Type>();
        collect(this);
        void collect(IEntityTypeTreeNode child)
        {
            ChildTypes.Add(child.EntityType);
            foreach (var childChild in child.Children)
            {
                collect(childChild);
            }
        }
    }

    public void Propagate(Entity e, EntityNotice noticeType)
    {
        var entityType = e.GetType();
        Handle(e, noticeType);
        Parent?.BubbleUp(e, entityType, noticeType);
        PushDown(e, entityType, noticeType);
    }

    private void Handle(Entity e, EntityNotice entityNotice)
    {
        if (entityNotice == EntityNotice.Creation)
        {
            Entities.Add((T) e);
            _created?.Invoke((T)e);
        }
        else if (entityNotice == EntityNotice.Destruction)
        {
            Entities.Remove((T) e);
            _destroyed?.Invoke((T)e);
        }
        else throw new Exception();
    }
    public void BubbleUp(Entity e, Type entityType, EntityNotice noticeType)
    {
        Handle(e, noticeType);
        Parent?.BubbleUp(e, entityType, noticeType);
    }
    public void BubbleDown(Entity e, Type entityType, EntityNotice noticeType)
    {
        Handle(e, noticeType);
        PushDown(e, entityType, noticeType);
    }
    public void PushDown(Entity e, Type entityType, EntityNotice noticeType)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].ChildTypes.Contains(entityType))
            {
                Children[i].BubbleDown(e, entityType, noticeType);
                break;
            }
        }
    }
    public void SetParent(IEntityTypeTreeNode parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
