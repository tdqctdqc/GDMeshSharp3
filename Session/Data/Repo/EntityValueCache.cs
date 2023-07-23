using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityValueCache<TEntity, TValue> : AuxData<TEntity>
    where TEntity : Entity
{
    public TValue this[TEntity t] => _dic.ContainsKey(t) ? _dic[t] : default;
    public IReadOnlyDictionary<TEntity, TValue> Dic => _dic;
    protected Dictionary<TEntity, TValue> _dic;
    protected Func<TEntity, TValue> _get;
    
    public static EntityValueCache<TEntity, TValue> ConstructConstant(Data data, Func<TEntity, TValue> get)
    {
        return new EntityValueCache<TEntity, TValue>(data, get);
    }
    public static EntityValueCache<TEntity, TValue> ConstructTrigger(Data data, Func<TEntity, TValue> get, 
        RefAction[] recalcTriggers, RefAction<Tuple<TEntity, TValue>>[] updateTriggers)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, recalcTriggers, updateTriggers);
    }

    private EntityValueCache(Data data, Func<TEntity, TValue> get) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        Initialize(data);
    }
    private EntityValueCache(Data data, Func<TEntity, TValue> get, RefAction[] recalcTriggers,
        RefAction<Tuple<TEntity, TValue>>[] updateTriggers) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        foreach (var trigger in recalcTriggers)
        {
            trigger.Subscribe(() =>
            {
                Initialize(data);
            });
        }
        foreach (var trigger in updateTriggers)
        {
            trigger.Subscribe(v =>
            {
                UpdateEntry(v.Item1, v.Item2);
            });
        }
        Initialize(data);
    }
    private void Initialize(Data data)
    {
        var entities = data.GetAll<TEntity>();
        _dic.Clear();
        foreach (var e in entities)
        {
            var v = _get((TEntity) e);
            if (v != null)
            {
                _dic.Add((TEntity)e, v);
            }
        }
    }

    private void UpdateEntry(TEntity e, TValue newVal)
    {
        _dic[e] = newVal;
    }
    public override void HandleAdded(TEntity added)
    {
        _dic[added] = _get(added);
    }

    public override void HandleRemoved(TEntity removing)
    {
        _dic.Remove(removing);
    }
}
