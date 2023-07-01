using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

public class AuxMultiIndexer<TSingle, TMult> : AuxData<TMult>
    where TMult : Entity
{
    public List<TMult> this[TSingle s] => _dic.ContainsKey(s) 
        ? _dic[s].ToList() 
        : null;
    protected Dictionary<TSingle, HashSet<TMult>> _dic;
    private Func<TMult, TSingle> _getSingle;
    public static AuxMultiIndexer<TSingle, TMult> ConstructConstant(Data data, Func<TMult, TSingle> getSingle)
    {
        return new AuxMultiIndexer<TSingle, TMult>(data, getSingle);
    }
    private AuxMultiIndexer(Data data, Func<TMult, TSingle> getSingle) : base(data)
    {
        _dic = new Dictionary<TSingle, HashSet<TMult>>();
        _getSingle = getSingle;
    }
    
    public override void HandleAdded(TMult added)
    {
        var single = _getSingle(added);
        if(single != null) _dic.AddOrUpdate(single, added);
    }
    public override void HandleRemoved(TMult removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single].Remove(removing);
    }
}
