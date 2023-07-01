using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class AddToModelRefColProcedure : AddToRefColProcedure<string>
{
    public static AddToModelRefColProcedure Construct(EntityRef<Entity> entity, string collectionName, 
        List<string> toAdd)
    {
        return new AddToModelRefColProcedure(entity, collectionName, toAdd);
    }
    [SerializationConstructor] private AddToModelRefColProcedure(EntityRef<Entity> entity, string collectionName, 
        List<string> toAdd) : base(entity, collectionName, toAdd)
    {
    }
}
