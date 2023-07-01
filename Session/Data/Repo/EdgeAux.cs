
using System;
using System.Collections.Generic;

public class EdgeAux<TEntity, TEnd> : AuxData<TEntity>
    where TEntity : Entity
{
    public TEntity this[TEnd t1, TEnd t2] => Get(t1, t2);
    private Dictionary<Edge<TEnd>, TEntity> _byEdge;
    
    private Func<TEnd, float> _getId;
    private Func<TEntity, Tuple<TEnd, TEnd>> _getEnds;

    public EdgeAux(Data data, Func<TEnd, float> getId, Func<TEntity, Tuple<TEnd, TEnd>> getEnds) : base(data)
    {
        _getId = getId;
        _getEnds = getEnds;
        _byEdge = new Dictionary<Edge<TEnd>, TEntity>();
    }

    public override void HandleAdded(TEntity added)
    {
        var edge = GetEdge(added);
        _byEdge.Add(edge, added);
    }

    public override void HandleRemoved(TEntity removing)
    {
        _byEdge.Remove(GetEdge(removing));
    }

    private Edge<TEnd> GetEdge(TEntity t)
    {
        var ends = _getEnds(t);
        return new Edge<TEnd>(ends.Item1, ends.Item2, _getId);
    }

    private TEntity Get(TEnd e1, TEnd e2)
    {
        var edge = new Edge<TEnd>(e1, e2, _getId);
        return _byEdge[edge];
    }
    public bool Contains(TEnd e1, TEnd e2)
    {
        var edge = new Edge<TEnd>(e1, e2, _getId);
        return _byEdge.ContainsKey(edge);
    }
}
