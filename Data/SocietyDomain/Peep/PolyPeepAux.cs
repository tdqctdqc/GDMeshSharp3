using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyPeepAux
{
    public EntityPropEntityIndexer<Peep, MapPolygon> ByPoly { get; private set; } 
    public PolyPeepAux(Data data)
    {
        ByPoly = EntityPropEntityIndexer<Peep, MapPolygon>
            .CreateStatic(data, p => p.Poly);
    }
}