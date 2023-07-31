using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AlliancePolyFill : PolyFillChunkGraphic
{
    public AlliancePolyFill(MapChunk chunk, Data data) 
        : base(nameof(AlliancePolyFill), chunk, GetColor, data)
    {
    }

    private static Color GetColor(MapPolygon poly, Data d)
    {
        if(poly.Regime.Fulfilled() == false) return Colors.Transparent;
        return poly.Regime.Entity(d).GetAlliance(d).Leader.Entity(d).PrimaryColor;
    }
}
