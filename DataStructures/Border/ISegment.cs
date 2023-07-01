using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ISegment<TPrim>
{
    TPrim From { get; }
    TPrim To { get; }
    ISegment<TPrim> ReverseGeneric();
    bool PointsTo(ISegment<TPrim> s);
    bool ComesFrom(ISegment<TPrim> s);
}

