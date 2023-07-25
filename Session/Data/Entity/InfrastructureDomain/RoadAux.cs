using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadAux
{
    public EntityPropEntityIndexer<RoadSegment, MapPolygonEdge> ByEdgeId { get; private set; }
    public RoadAux(Data data)
    {
        ByEdgeId = EntityPropEntityIndexer<RoadSegment, MapPolygonEdge>.CreateStatic(data, rs => rs.Edge);
    }
}