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
    public ResourceDepositAux ResourceDepositAux { get; private set; }

    public float Width => _planetInfoAux.Value.Dimensions.X;
    public float Height => _planetInfoAux.Value.Dimensions.Y;
    public PlanetDomain(Data data) : base(typeof(PlanetDomain), data)
    {
        
    }
    public override void Setup()
    {
        _planetInfoAux = new SingletonAux<PlanetInfo>(Data);
        PolygonAux = new MapPolygonAux(Data);
        PolyEdgeAux = new PolyEdgeAux(Data);
        ResourceDepositAux = new ResourceDepositAux(Data);
    }
}