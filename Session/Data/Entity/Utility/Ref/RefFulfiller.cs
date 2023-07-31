using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RefFulfiller
{
    private Data _data;
    private Dictionary<int, List<IRef>> _entityRefs;

    public RefFulfiller(Data data)
    {
        _data = data;
        _entityRefs = new Dictionary<int, List<IRef>>();
    }
    

    public void EntityRemoved(int id)
    {
        if (_entityRefs.ContainsKey(id))
        {
            _entityRefs[id].ForEach(r => r.ClearRef());
            _entityRefs.Remove(id);
        }
    }
    public TEntity Get<TEntity>(int id) where TEntity : Entity
    {
        return (TEntity) _data.EntitiesById[id];
    }
    public TEntity Get<TEntity>(EntityRef<TEntity> tRef) where TEntity : Entity
    {
        return (TEntity) _data.EntitiesById[tRef.RefId];
    }
}