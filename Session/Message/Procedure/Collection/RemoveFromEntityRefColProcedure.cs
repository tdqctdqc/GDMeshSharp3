
using System.Collections.Generic;
using MessagePack;

public class RemoveFromEntityRefColProcedure : RemoveFromRefColProcedure<int>
{
    public static RemoveFromEntityRefColProcedure Create(Entity e, string colName, List<int> toAdd)
    {
        return new RemoveFromEntityRefColProcedure(e.MakeRef(), colName, toAdd);
    }
    [SerializationConstructor] private RemoveFromEntityRefColProcedure(EntityRef<Entity> entity, string collectionName, List<int> toRemove)
        : base(entity, collectionName, toRemove)
    {
    }
}
