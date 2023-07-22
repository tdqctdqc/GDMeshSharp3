using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadAux : EntityAux<RoadSegment>
{
    public EntityPropEntityIndexer<RoadSegment, MapPolygonEdge> ByEdgeId { get; private set; }
    public RoadAux(Data data) : base(data)
    {
        ByEdgeId = EntityPropEntityIndexer<RoadSegment, MapPolygonEdge>.CreateStatic(data, rs => rs.Edge);
    }
}