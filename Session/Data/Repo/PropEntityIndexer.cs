
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PropEntityIndexer<TEntity, TKey> : AuxData<TEntity>
    where TEntity : Entity
{
    public TEntity this[TKey e] => _dic.ContainsKey(e) ? _dic[e] : null;
    protected Dictionary<TKey, TEntity> _dic;
    protected Func<TEntity, TKey> _get;
    
    public static PropEntityIndexer<TEntity, TKey> CreateConstant(Data data, Func<TEntity, TKey> get)
    {
        return new PropEntityIndexer<TEntity, TKey>(data, get);
    }
    public static PropEntityIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, TKey> get,
        params ValChangeAction<TKey>[] changedValTriggers)
    {
        return new PropEntityIndexer<TEntity, TKey>(data, get, changedValTriggers);
    }
    protected PropEntityIndexer(Data data, Func<TEntity, TKey> get,
        params ValChangeAction<TKey>[] changedValTriggers) : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TEntity>();
        foreach (var trigger in changedValTriggers)
        {
            trigger.Subscribe(n =>
            {
                var t = (TEntity) n.Entity;
                if (n.OldVal != null)
                {
                    if (_dic.ContainsKey(n.OldVal) == false) throw new Exception();
                    if (_dic[n.OldVal] == t)
                    {
                        _dic.Remove(n.OldVal);
                    }
                }
                _dic[n.NewVal] = (TEntity)n.Entity;
            });
        }
        // Initialize(data);
    }
    private void Initialize(Data data)
    {
        _dic = new Dictionary<TKey, TEntity>();
        var register = data.GetRegister<TEntity>();
        foreach (var e in register.Entities)
        {
            _dic.Add(_get((TEntity)e), (TEntity)e);
        }
    }
    public bool ContainsKey(TKey e)
    {
        return _dic.ContainsKey(e);
    }
    public override void HandleAdded(TEntity added)
    {
        var val = _get(added);
        if(val != null)
        {
            _dic[val] = added;
        }
    }

    public override void HandleRemoved(TEntity removing)
    {
        var val = _get(removing);
        if (val != null) _dic.Remove(val);
    }
}
