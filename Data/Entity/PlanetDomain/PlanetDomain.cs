using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain
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
    private Data _data;
    public PlanetDomain(Data data)
    {
        _data = data;
    }
    public void Setup()
    {
        _planetInfoAux = new SingletonAux<PlanetInfo>(_data);
        _polyNav = new SingletonAux<Nav>(_data);
        PolygonAux = new MapPolygonAux(_data);
        PolyEdgeAux = new PolyEdgeAux(_data);
        ResourceDepositAux = new ResourceDepositAux(_data);
    }
    
    public Vector2 GetOffsetTo(Vector2 p1, Vector2 p2)
    {
        var w = _data.Planet.Width;
        var off1 = p2 - p1;
        var off2 = (off1 + Vector2.Right * w);
        var off3 = (off1 + Vector2.Left * w);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }

    public Vector2 ClampPosition(Vector2 pos)
    {
        return GetOffsetTo(Vector2.Zero, pos);
    }
}