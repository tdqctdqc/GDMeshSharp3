using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyPeepAux : EntityAux<PolyPeep>
{
    public Entity1To1Indexer<PolyPeep, MapPolygon> ByPoly { get; private set; } 
    public PolyPeepAux(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = Entity1To1Indexer<PolyPeep, MapPolygon>
            .CreateStatic(data, p => p.Poly);
    }
}