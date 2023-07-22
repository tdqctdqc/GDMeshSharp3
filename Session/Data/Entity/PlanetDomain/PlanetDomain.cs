using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public EntityRegister<MapPolygon> Polygons => Data.GetRegister<MapPolygon>();
    public MapPolygonAux PolygonAux { get; private set; }
    public EntityRegister<MapPolygonEdge> PolyEdges => Data.GetRegister<MapPolygonEdge>();

    public PolyEdgeAux PolyEdgeAux { get; private set; }
    public PlanetInfo Info => _planetInfoAux != null ? _planetInfoAux.Value : null;
    private SingletonAux<PlanetInfo> _planetInfoAux;
    public EntityRegister<ResourceDeposit> ResourceDeposits => Data.GetRegister<ResourceDeposit>();
    public EntityRegister<MapPolyNexus> PolyNexi => Data.GetRegister<MapPolyNexus>();
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