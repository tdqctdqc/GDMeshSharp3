using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
using VoronoiSandbox;

public class MapPolyNexus : Entity
{
    public Vector2 Point { get; private set; }
    public ERefSet<MapPolygonEdge> IncidentEdges { get; private set; }
    public ERefSet<MapPolygon> IncidentPolys { get; private set; }
    public static MapPolyNexus Create(PreNexus pre,
        GenKey key)
    {
        var id = pre.Id;
        var ps = new HashSet<int>();
        if (pre.P1 != null) ps.Add(pre.P1.Id);
        if (pre.P2 != null) ps.Add(pre.P2.Id);
        if (pre.P3 != null) ps.Add(pre.P3.Id);

        var es = new HashSet<int>();
        if (pre.E1 != null) es.Add(pre.E1.Id);
        if (pre.E2 != null) es.Add(pre.E2.Id);
        if (pre.E3 != null) es.Add(pre.E3.Id);
        
        var n = new MapPolyNexus(id, pre.Pos, 
            ERefSet<MapPolygonEdge>.Construct(es),
            ERefSet<MapPolygon>.Construct(ps)
            );
        
        key.Create(n);
        return n;
    }
    
    
    public static MapPolyNexus Create(Vector2 pos, MapPolygon p1, MapPolygon p2,
        GenKey key)
    {

        var mutual = p1.Neighbors.Entities(key.Data)
            .Intersect(p2.Neighbors.Entities(key.Data)).ToArray();
        if (mutual.Length != 1) throw new Exception();
        var p3 = mutual[0];
        var e1 = p1.GetEdge(p2, key.Data);
        var e2 = p2.GetEdge(p3, key.Data);
        var e3 = p3.GetEdge(p1, key.Data);
        var id = key.Data.IdDispenser.TakeId();
        var n = new MapPolyNexus(id,
            pos, 
            ERefSet<MapPolygonEdge>.Construct(
                new HashSet<ERef<MapPolygonEdge>>{e1.MakeRef(), e2.MakeRef(), e3.MakeRef()}),
            
            ERefSet<MapPolygon>.Construct(new HashSet<ERef<MapPolygon>>{p1.MakeRef(), p2.MakeRef(), p3.MakeRef()})
        );
        
        key.Create(n);
        return n;
    }
    
    
    [SerializationConstructor] private MapPolyNexus(int id, Vector2 point, 
        ERefSet<MapPolygonEdge> incidentEdges,
        ERefSet<MapPolygon> incidentPolys) : base(id)
    {
        Point = point;
        IncidentEdges = ERefSet<MapPolygonEdge>.Construct(incidentEdges.Refs);
        IncidentPolys = ERefSet<MapPolygon>.Construct(incidentPolys.Refs);
    }

    public MapPolygonEdge GetEdgeWith(MapPolyNexus n, Data data)
    {
        return IncidentEdges.Entities(data).First(e => e.HiNexus.Get(data) == n
                                    || e.LoNexus.Get(data) == n);
    }

    public IEnumerable<MapPolyNexus> GetNeighbors(Data data)
    {
        return IncidentEdges.Entities(data).Select(e =>
        {
            if (e.HiNexus.Get(data) == this) return e.LoNexus.Get(data);
            return e.HiNexus.Get(data);
        });
    }
    

    public void SetPoint(Vector2 point, GenKey key)
    {
        Point = point;
    }

    public override void CleanUp(ProcedureKey key)
    {
        
    }
}
