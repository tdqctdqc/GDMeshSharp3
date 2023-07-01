
using System.Collections.Generic;
using MessagePack;

public class AddToEntityRefColProcedure : AddToRefColProcedure<int>
{

    public static AddToEntityRefColProcedure Create(Entity e, string colName, List<int> toAdd)
    {
        return new AddToEntityRefColProcedure(e.MakeRef(), colName, toAdd);
    }
    [SerializationConstructor] 
    private AddToEntityRefColProcedure(EntityRef<Entity> entity, string collectionName, List<int> toAdd)
        : base(entity, collectionName, toAdd)
    {
    }
}
