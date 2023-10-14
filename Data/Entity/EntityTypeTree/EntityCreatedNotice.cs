using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Extensions.ObjectPool;

public class EntityCreatedNotice : IEntityTypeTreeNotice
{
    private static ObjectPool<EntityCreatedNotice> _pool =
        new DefaultPool<EntityCreatedNotice>(() => new EntityCreatedNotice());
    public Entity Entity { get; private set; }
    public Type EntityType => Entity.GetType();
    private EntityCreatedNotice()
    {
    }
    public static EntityCreatedNotice Get(Entity entity)
    {
        var n = _pool.Get();
        n.Setup(entity);
        return n;
    }

    public void Setup(Entity entity)
    {
        Entity = entity;
    }

    public void Return()
    {
        Entity = null;
        _pool.Return(this);
    }
    public void HandleForTreeNode(IEntityTypeTreeNode node)
    {
        node.AddEntity(Entity);
        node.Created.Invoke(this);
    }
}


