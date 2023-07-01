using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RemoveFromModelRefColProcedure : RemoveFromRefColProcedure<string>
{
    public static RemoveFromModelRefColProcedure Construct(EntityRef<Entity> entity, string collectionName, 
        List<string> toAdd)
    {
        return new RemoveFromModelRefColProcedure(entity, collectionName, toAdd);
    }
    [SerializationConstructor] private RemoveFromModelRefColProcedure(EntityRef<Entity> entity, string collectionName, 
        List<string> toAdd) : base(entity, collectionName, toAdd)
    {
    }
}
