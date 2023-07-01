
using System;

public class EntityDestroyedNotice : IEntityNotice
{
    public Entity Entity { get; private set; }
    Type IEntityNotice.EntityType => Entity.GetType();
    

    public EntityDestroyedNotice(Entity entity)
    {
        Entity = entity;
    }
    public void Clear()
    {
        Entity = null;
    }
}
