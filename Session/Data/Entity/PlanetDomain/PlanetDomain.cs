using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public MapPolygonAux PolygonAux { get; private set; }

    public PolyEdgeAux PolyEdgeAux { get; private set; }
    public PlanetInfo Info => _planetInfoAux != null ? _planetInfoAux.Value : null;
    private SingletonAux<PlanetInfo> _planetInfoAux;
    public Nav Nav => _polyNav.Value;
    private SingletonAux<Nav> _polyNav;

    public ResourceDepositAux ResourceDepositAux { get; private set; }

    public float Width => _planetInfoAux.Value.Dimensions.X;
    public float Height => _planetInfoAux.Value.Dimensions.Y;
    public PlanetDomain(Data data) : base(typeof(PlanetDomain), data)
    {
        
    }
    public override void Setup()
    {
        _planetInfoAux = new SingletonAux<PlanetInfo>(Data);
        _polyNav = new SingletonAux<Nav>(Data);
        PolygonAux = new MapPolygonAux(Data);
        PolyEdgeAux = new PolyEdgeAux(Data);
        ResourceDepositAux = new ResourceDepositAux(Data);
    }
    
    public static Vector2 GetOffsetTo(Vector2 p1, Vector2 p2, Data data)
    {
        var w = data.Planet.Width;
        var off1 = p2 - p1;
        var off2 = (off1 + Vector2.Right * w);
        var off3 = (off1 + Vector2.Left * w);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }

    public static Vector2 ClampPosition(Vector2 pos, Data data)
    {
        return GetOffsetTo(Vector2.Zero, pos, data);
    }
}