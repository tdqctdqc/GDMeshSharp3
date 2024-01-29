using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct BorderEdge<TNode> : ISegment<TNode>
{
    public TNode Native { get; set; }
    public TNode Foreign { get; set; }

    public BorderEdge(TNode native, TNode foreign)
    {
        Native = native;
        Foreign = foreign;
    }

    TNode ISegment<TNode>.From => Native;

    TNode ISegment<TNode>.To => Foreign;

    ISegment<TNode> ISegment<TNode>.ReverseGeneric() => new BorderEdge<TNode>(Foreign, Native);

    bool ISegment<TNode>.PointsTo(ISegment<TNode> s)
    {
        return s.From.Equals(Foreign);
    }

    bool ISegment<TNode>.ComesFrom(ISegment<TNode> s)
    {
        return s.To.Equals(Native);
    }
}