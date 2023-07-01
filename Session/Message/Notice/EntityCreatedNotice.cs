
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Extensions.ObjectPool;


public class EntityCreatedNotice : IEntityNotice
{
    private static ObjectPool<EntityCreatedNotice> _pool = SetupPool();
    public IReadOnlyList<Entity> Entities { get; private set; }
    public Type EntityType { get; private set; }

    public static EntityCreatedNotice ConstructMultiple<TEntity>(IReadOnlyList<TEntity> entities)
        where TEntity : Entity
    {
        var n = _pool.Get();
        n.Setup<TEntity>(entities);
        return n;
    }
    public static EntityCreatedNotice Construct(Entity entity)
    {
        var n = _pool.Get();
        n.Setup(entity);
        return n;
    }

    public void Setup(Entity entity)
    {
        Entities = new List<Entity>{entity};
        EntityType = entity.GetType();
    }
    public void Setup<TEntity>(IReadOnlyList<TEntity> entities) where TEntity : Entity
    {
        Entities = entities;
        EntityType = typeof(TEntity);
    }

    public void Return()
    {
        _pool.Return(this);
    }
    public void Clear()
    {
        Entities = null;
        EntityType = null;
    }
    private EntityCreatedNotice()
    {
    }

    private static ObjectPool<EntityCreatedNotice> SetupPool()
    {
        var policy = new NoticePolicy<EntityCreatedNotice>(() => new EntityCreatedNotice());
        var pool = new DefaultObjectPool<EntityCreatedNotice>(policy);
        return pool;
    }
}

public class NoticePolicy<T> : PooledObjectPolicy<T> where T : IEntityNotice
{
    private Func<T> _constructor;

    public NoticePolicy(Func<T> constructor) : base()
    {
        _constructor = constructor;
    }

    public override T Create()
    {
        return _constructor();
    }

    public override bool Return(T obj)
    {
        obj.Clear();
        return true;
    }
}
