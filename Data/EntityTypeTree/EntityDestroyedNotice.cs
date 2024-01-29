
using System;
using Microsoft.Extensions.ObjectPool;

public class EntityDestroyedNotice : IEntityTypeTreeNotice
{
    private static ObjectPool<EntityDestroyedNotice> _pool 
        = new DefaultPool<EntityDestroyedNotice>(() => new EntityDestroyedNotice());
    public Entity Entity { get; private set; }
    Type IEntityTypeTreeNotice.EntityType => Entity.GetType();

    public static EntityDestroyedNotice Get(Entity entity)
    {
        var n = _pool.Get();
        n.Setup(entity);
        return n;
    }
    private EntityDestroyedNotice()
    {
    }
    public void Setup(Entity entity)
    {
        Entity = entity;
    }

    public void HandleForTreeNode(IEntityTypeTreeNode node)
    {
        node.RemoveEntity(Entity);
        node.Destroyed.Invoke(this);
    }

    public void Return()
    {
        Entity = null;
        _pool.Return(this);
    }
}
