using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadAux : EntityAux<RoadSegment>
{
    public Entity1To1Indexer<RoadSegment, MapPolygonEdge> ByEdgeId { get; private set; }
    public RoadAux(Domain domain, Data data) : base(domain, data)
    {
        ByEdgeId = Entity1To1Indexer<RoadSegment, MapPolygonEdge>.CreateStatic(data, rs => rs.Edge);
    }
}