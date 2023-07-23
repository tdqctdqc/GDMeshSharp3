using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityTypeTreeNode
{
    public Type EntityType { get; private set; }
    public IEntityMeta Meta { get; private set; }
    public EntityTypeTreeNode Parent { get; private set; }
    public List<EntityTypeTreeNode> Children { get; private set; }
    public RefAction<EntityCreatedNotice> Created { get; private set; }    
    public RefAction<EntityDestroyedNotice> Destroyed { get; private set; }
    public HashSet<Entity> Entities { get; private set; }
    public EntityTypeTreeNode(Type entityType)
    {
        EntityType = entityType;
        Children = new List<EntityTypeTreeNode>();
        Created = new RefAction<EntityCreatedNotice>();
        Destroyed = new RefAction<EntityDestroyedNotice>();
        Entities = new HashSet<Entity>();
        Meta = IEntityMeta.ConstructFromType(entityType);
    }

    public void PropagateCreation(Entity e)
    {
        Entities.Add(e);
        var n = EntityCreatedNotice.Construct(e);
        Parent?.BubbleUp(n);
        Created.Invoke(n);
        PushDown(n);
        n.Return();
    }
    public void PropagateCreations<TEntity>(IReadOnlyList<TEntity> es)
        where TEntity : Entity
    {
        Entities.AddRange(es);
        var n = EntityCreatedNotice.ConstructMultiple<TEntity>(es);
        Parent?.BubbleUp(n);
        Created.Invoke(n);
        PushDown(n);
        n.Return();
    }
    private void PushDown(EntityCreatedNotice notice)
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
    private void BubbleUp(EntityCreatedNotice notice)
    {
        Entities.AddRange(notice.Entities);
        Parent?.BubbleUp(notice);
        Created.Invoke(notice);
    }
    private void BubbleDown(EntityCreatedNotice notice)
    {
        Entities.AddRange(notice.Entities);
        Created.Invoke(notice);
        PushDown(notice);
    }
    
    
    public void PropagateDestruction(Entity e)
    {
        Entities.Remove(e);
        var n = new EntityDestroyedNotice(e); 
        Parent?.BubbleUp(n);
        Destroyed.Invoke(n);
        PushDown(n);
    }
    
    private void PushDown(EntityDestroyedNotice notice)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            if (notice.Entity.GetType().IsAssignableFrom(Children[i].EntityType))
            {
                Children[i].BubbleDown(notice);
                break;
            }
        }
    }
    private void BubbleUp(EntityDestroyedNotice notice)
    {
        Entities.Remove(notice.Entity);
        Parent?.BubbleUp(notice);
        Destroyed.Invoke(notice);
    }
    private void BubbleDown(EntityDestroyedNotice notice)
    {
        Entities.Remove(notice.Entity);
        Destroyed.Invoke(notice);
        PushDown(notice);
    }
    public void SetParent(EntityTypeTreeNode parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
