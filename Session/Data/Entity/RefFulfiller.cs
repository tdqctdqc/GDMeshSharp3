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


    public void Fulfill(IRef r)
    {
        if (r is IEntityRef i && i.RefId != -1) _entityRefs.AddOrUpdate(i.RefId, r);
        r.SyncRef(_data);
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
        return (TEntity) _data.Entities[id];
    }
    public TEntity Get<TEntity>(EntityRef<TEntity> tRef) where TEntity : Entity
    {
        return (TEntity) _data.Entities[tRef.RefId];
    }
}