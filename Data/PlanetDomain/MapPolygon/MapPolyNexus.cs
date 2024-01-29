using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class MapPolyNexus : Entity
{
    public Vector2 Point { get; private set; }
    public EntRefCol<MapPolygonEdge> IncidentEdges { get; private set; }
    public EntRefCol<MapPolygon> IncidentPolys { get; private set; }
    public static MapPolyNexus Create(Vector2 point, List<MapPolygonEdge> edges, List<MapPolygon> polys,
        GenWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var n = new MapPolyNexus(id, point, 
            EntRefCol<MapPolygonEdge>.Construct(nameof(IncidentEdges), id,
                edges.Select(e => e.Id).ToHashSet(), key.Data),
            EntRefCol<MapPolygon>.Construct(nameof(IncidentPolys), id,
                polys.Select(p => p.Id).ToHashSet(), key.Data));
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private MapPolyNexus(int id, Vector2 point, EntRefCol<MapPolygonEdge> incidentEdges,
        EntRefCol<MapPolygon> incidentPolys) : base(id)
    {
        Point = point;
        IncidentEdges = EntRefCol<MapPolygonEdge>.Construct(incidentEdges);
        IncidentPolys = EntRefCol<MapPolygon>.Construct(incidentPolys);
    }

    public MapPolygonEdge GetEdgeWith(MapPolyNexus n, Data data)
    {
        return IncidentEdges.Items(data).First(e => e.HiNexus.Entity(data) == n
                                    || e.LoNexus.Entity(data) == n);
    }

    public IEnumerable<MapPolyNexus> GetNeighbors(Data data)
    {
        return IncidentEdges.Items(data).Select(e =>
        {
            if (e.HiNexus.Entity(data) == this) return e.LoNexus.Entity(data);
            return e.HiNexus.Entity(data);
        });
    }
    

    public void SetPoint(Vector2 point, GenWriteKey key)
    {
        Point = point;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
