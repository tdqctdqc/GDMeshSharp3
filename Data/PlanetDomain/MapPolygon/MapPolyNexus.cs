using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class MapPolyNexus : Entity
{
    public Vector2 Point { get; private set; }
    public ERefSet<MapPolygonEdge> IncidentEdges { get; private set; }
    public ERefSet<MapPolygon> IncidentPolys { get; private set; }
    public static MapPolyNexus Create(PreNexus pre,
        GenWriteKey key)
    {
        var id = pre.Id;
        var p1 = key.Data.Get<MapPolygon>(pre.P1.Id);
        var p2 = key.Data.Get<MapPolygon>(pre.P2.Id);
        var p3 = key.Data.Get<MapPolygon>(pre.P3.Id);

        var e1 = p1.GetEdge(p2, key.Data);
        var e2 = p2.GetEdge(p3, key.Data);
        var e3 = p3.GetEdge(p1, key.Data);
        
        var n = new MapPolyNexus(id, pre.Pos, 
            ERefSet<MapPolygonEdge>.Construct(
                nameof(IncidentEdges), id,
                new HashSet<int>{e1.Id, e2.Id, e3.Id}, key.Data),
            
            ERefSet<MapPolygon>.Construct(nameof(IncidentPolys), id,
                new HashSet<int>{p1.Id, p2.Id, p3.Id}, key.Data)
            );
        
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private MapPolyNexus(int id, Vector2 point, ERefSet<MapPolygonEdge> incidentEdges,
        ERefSet<MapPolygon> incidentPolys) : base(id)
    {
        Point = point;
        IncidentEdges = ERefSet<MapPolygonEdge>.Construct(incidentEdges);
        IncidentPolys = ERefSet<MapPolygon>.Construct(incidentPolys);
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
