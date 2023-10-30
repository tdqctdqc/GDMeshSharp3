using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Extensions.ObjectPool;

public class EntityCreatedNotice : IEntityTypeTreeNotice
{
    public Entity Entity { get; private set; }
    public Type EntityType => Entity.GetType();
    private EntityCreatedNotice()
    {
    }
    public static EntityCreatedNotice Get(Entity entity)
    {
        if (entity == null) throw new Exception();
        var n = new EntityCreatedNotice();
        n.Setup(entity);
        return n;
    }

    public void Setup(Entity entity)
    {
        Entity = entity;
    }

    public void HandleForTreeNode(IEntityTypeTreeNode node)
    {
        node.AddEntity(Entity);
        node.Created.Invoke(this);
    }
}


