using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyPeepAux : EntityAux<PolyPeep>
{
    public EntityPropEntityIndexer<PolyPeep, MapPolygon> ByPoly { get; private set; } 
    public PolyPeepAux(Data data) : base(data)
    {
        ByPoly = EntityPropEntityIndexer<PolyPeep, MapPolygon>
            .CreateStatic(data, p => p.Poly);
    }
}