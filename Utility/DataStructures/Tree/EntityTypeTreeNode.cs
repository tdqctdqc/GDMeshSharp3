using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityTypeTreeNode
{
    public IEntityMeta Meta { get; private set; }
    public Type EntityType { get; private set; }
    public EntityTypeTreeNode Parent { get; private set; }
    public List<EntityTypeTreeNode> Children { get; private set; }
    public RefAction<EntityCreatedNotice> Created { get; private set; }    
     public RefAction<EntityDestroyedNotice> Destroyed { get; private set; }
    public EntityTypeTreeNode(Type entityType)
    {
        Meta = Game.I.Serializer.GetEntityMeta(entityType);
        EntityType = entityType;
        Children = new List<EntityTypeTreeNode>();
        Created = new RefAction<EntityCreatedNotice>();
        Destroyed = new RefAction<EntityDestroyedNotice>();
        var setTreeMethod = entityType.GetProperty(nameof(RoadSegment.EntityTypeTreeNode), 
                BindingFlags.Public | BindingFlags.Static)
            .SetMethod;
        setTreeMethod.Invoke(null, new object[]{this});
    }

    public void PropagateCreation(Entity e)
    {
        var n = EntityCreatedNotice.Construct(e);
        Parent?.BubbleUp(n);
        Created.Invoke(n);
        PushDown(n);
        n.Return();
    }
    public void PropagateCreations<TEntity>(IReadOnlyList<TEntity> es)
        where TEntity : Entity
    {
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
        if (RelevantField(notice) == false) return;
        Parent?.BubbleUp(notice);
        Created.Invoke(notice);
    }private void BubbleDown(EntityCreatedNotice notice)
    {
        Created.Invoke(notice);
        PushDown(notice);
    }
    
    
    public void PropagateDestruction(Entity e)
    {
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
        if (RelevantField(notice) == false) return;
        Parent?.BubbleUp(notice);
        Destroyed.Invoke(notice);
    }
    private void BubbleDown(EntityDestroyedNotice notice)
    {
        Destroyed.Invoke(notice);
        PushDown(notice);
    }
    
    private bool RelevantField(IEntityNotice n)
    {
        return n is ValChangeNotice v == false || Meta.FieldNameHash.Contains(v.FieldName);
    }
    public void SetParent(EntityTypeTreeNode parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
