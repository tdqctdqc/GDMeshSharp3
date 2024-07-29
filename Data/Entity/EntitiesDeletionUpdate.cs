using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntitiesDeletionUpdate : Update
{
    public int[] EntityIds { get; private set; }
    
    public static EntitiesDeletionUpdate Create(IEnumerable<int> entityIds, HostKey key)
    {
        return new EntitiesDeletionUpdate(entityIds.ToArray());
    }
    [SerializationConstructor] public EntitiesDeletionUpdate(int[] entityIds) 
    {
        EntityIds = entityIds;
    }
    public override void Enact(ProcedureKey key)
    {
        key.Data.RemoveEntities(EntityIds, key);
    }
}