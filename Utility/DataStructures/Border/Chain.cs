
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Chain<TSeg, TPrim> : IChain<TSeg, TPrim>, 
    ISegment<TPrim> 
    where TSeg : ISegment<TPrim>
{
    public TSeg this[int i] => Segments[i];
    public List<TSeg> Segments { get; private set; }
    public Chain(List<TSeg> segments)
    {
        Segments = segments;
    }
    public ISegment<TPrim> ReverseGeneric()
    {
        var r = Segments.Select(e => e.Reverse<TSeg, TPrim>()).ToList();
        r.Reverse();
        return new Chain<TSeg, TPrim>(r);
    }

    IReadOnlyList<TSeg> IChain<TSeg>.Segments => Segments;
    TPrim ISegment<TPrim>.From => Segments[0].From;
    TPrim ISegment<TPrim>.To => Segments[Segments.Count - 1].To;
    bool ISegment<TPrim>.PointsTo(ISegment<TPrim> s)
    {
        if (s is ISegment<TPrim> p)
        {
            return Segments[Segments.Count - 1].To.Equals(p.From);
        }

        return false;
    }
    bool ISegment<TPrim>.ComesFrom(ISegment<TPrim> s)
    {
        if (s is ISegment<TPrim> p) return Segments[0].From.Equals(p.To);
        return false;
    }
}
